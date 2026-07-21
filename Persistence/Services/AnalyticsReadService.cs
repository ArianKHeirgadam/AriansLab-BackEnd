using Application.DTOs.Analytics;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Globalization;

namespace Persistence.Services;

public sealed class AnalyticsReadService : IAnalyticsReadService
{
    private readonly ApplicationDbContext _dbContext;

    public AnalyticsReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnalyticsDashboardDto> GetDashboardAsync(
        int days,
        CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 7, 90);

        var endExclusive = DateTime.UtcNow.Date.AddDays(1);
        var start = endExclusive.AddDays(-days);
        var previousStart = start.AddDays(-days);

        var currentViews = _dbContext.PageViews
            .AsNoTracking()
            .Where(item => item.CreatedAt >= start && item.CreatedAt < endExclusive);
        var previousViews = _dbContext.PageViews
            .AsNoTracking()
            .Where(item => item.CreatedAt >= previousStart && item.CreatedAt < start);
        var customers = _dbContext.Users
            .AsNoTracking()
            .Where(item => item.Role == UserRole.Customer);
        var paidPayments = _dbContext.Payments
            .AsNoTracking()
            .Where(item => item.Status == PaymentStatus.Paid && item.PaidAt.HasValue);

        var totalPageViews = await _dbContext.PageViews.LongCountAsync(cancellationToken);
        var periodPageViews = await currentViews.CountAsync(cancellationToken);
        var previousPageViews = await previousViews.CountAsync(cancellationToken);
        var uniqueVisitors = await currentViews
            .Select(item => item.VisitorIdHash)
            .Distinct()
            .CountAsync(cancellationToken);
        var previousUniqueVisitors = await previousViews
            .Select(item => item.VisitorIdHash)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalUsers = await customers.CountAsync(cancellationToken);
        var newRegistrations = await customers
            .CountAsync(item => item.CreatedAt >= start && item.CreatedAt < endExclusive, cancellationToken);
        var previousRegistrations = await customers
            .CountAsync(item => item.CreatedAt >= previousStart && item.CreatedAt < start, cancellationToken);
        var totalProjects = await _dbContext.Projects.CountAsync(cancellationToken);
        var unreadMessages = await _dbContext.ContactMessages
            .CountAsync(item => !item.IsRead, cancellationToken);
        var openTickets = await _dbContext.SupportTickets
            .CountAsync(item => item.Status != TicketStatus.Closed, cancellationToken);

        var paidRevenue = await paidPayments
            .Where(item => item.PaidAt >= start && item.PaidAt < endExclusive)
            .Select(item => (decimal?)item.Amount)
            .SumAsync(cancellationToken) ?? 0m;
        var previousRevenue = await paidPayments
            .Where(item => item.PaidAt >= previousStart && item.PaidAt < start)
            .Select(item => (decimal?)item.Amount)
            .SumAsync(cancellationToken) ?? 0m;

        var dailyViews = await currentViews
            .GroupBy(item => item.CreatedAt.Date)
            .Select(group => new
            {
                Date = group.Key,
                PageViews = group.Count(),
                UniqueVisitors = group.Select(item => item.VisitorIdHash).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        var dailyRegistrations = await customers
            .Where(item => item.CreatedAt >= start && item.CreatedAt < endExclusive)
            .GroupBy(item => item.CreatedAt.Date)
            .Select(group => new { Date = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var dailyRevenue = await paidPayments
            .Where(item => item.PaidAt >= start && item.PaidAt < endExclusive)
            .GroupBy(item => item.PaidAt!.Value.Date)
            .Select(group => new { Date = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToListAsync(cancellationToken);

        var viewsByDate = dailyViews.ToDictionary(item => item.Date);
        var registrationsByDate = dailyRegistrations.ToDictionary(item => item.Date, item => item.Count);
        var revenueByDate = dailyRevenue.ToDictionary(item => item.Date, item => item.Amount);
        var daily = new List<AnalyticsDailyPointDto>(days);

        for (var offset = 0; offset < days; offset++)
        {
            var date = start.AddDays(offset);
            viewsByDate.TryGetValue(date, out var viewPoint);
            daily.Add(new AnalyticsDailyPointDto
            {
                Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                PageViews = viewPoint?.PageViews ?? 0,
                UniqueVisitors = viewPoint?.UniqueVisitors ?? 0,
                Registrations = registrationsByDate.GetValueOrDefault(date),
                PaidRevenue = revenueByDate.GetValueOrDefault(date)
            });
        }

        var topPages = await currentViews
            .GroupBy(item => item.Path)
            .Select(group => new AnalyticsPageDto
            {
                Path = group.Key,
                PageViews = group.Count(),
                UniqueVisitors = group.Select(item => item.VisitorIdHash).Distinct().Count()
            })
            .OrderByDescending(item => item.PageViews)
            .ThenBy(item => item.Path)
            .Take(8)
            .ToListAsync(cancellationToken);

        var trafficSources = await currentViews
            .GroupBy(item => item.ReferrerHost ?? "direct")
            .Select(group => new AnalyticsBreakdownDto
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .Take(8)
            .ToListAsync(cancellationToken);

        var devices = await currentViews
            .GroupBy(item => item.DeviceType)
            .Select(group => new AnalyticsBreakdownDto
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ToListAsync(cancellationToken);

        var browsers = await currentViews
            .GroupBy(item => item.Browser)
            .Select(group => new AnalyticsBreakdownDto
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .Take(6)
            .ToListAsync(cancellationToken);

        AddPercentages(trafficSources, periodPageViews);
        AddPercentages(devices, periodPageViews);
        AddPercentages(browsers, periodPageViews);

        return new AnalyticsDashboardDto
        {
            Days = days,
            From = DateTime.SpecifyKind(start, DateTimeKind.Utc),
            To = DateTime.SpecifyKind(endExclusive.AddTicks(-1), DateTimeKind.Utc),
            Overview = new AnalyticsOverviewDto
            {
                TotalPageViews = totalPageViews,
                PeriodPageViews = periodPageViews,
                UniqueVisitors = uniqueVisitors,
                TotalUsers = totalUsers,
                NewRegistrations = newRegistrations,
                TotalProjects = totalProjects,
                PaidRevenue = paidRevenue,
                UnreadMessages = unreadMessages,
                OpenTickets = openTickets,
                PageViewsChangePercent = CalculateChange(periodPageViews, previousPageViews),
                UniqueVisitorsChangePercent = CalculateChange(uniqueVisitors, previousUniqueVisitors),
                RegistrationsChangePercent = CalculateChange(newRegistrations, previousRegistrations),
                RevenueChangePercent = CalculateChange(paidRevenue, previousRevenue)
            },
            Daily = daily,
            TopPages = topPages,
            TrafficSources = trafficSources,
            Devices = devices,
            Browsers = browsers
        };
    }

    private static void AddPercentages(
        IEnumerable<AnalyticsBreakdownDto> items,
        int total)
    {
        foreach (var item in items)
        {
            item.Percentage = total == 0
                ? 0
                : Math.Round(item.Count * 100d / total, 1);
        }
    }

    private static double CalculateChange(decimal current, decimal previous)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : 100;
        }

        return Math.Round((double)((current - previous) * 100 / previous), 1);
    }
}
