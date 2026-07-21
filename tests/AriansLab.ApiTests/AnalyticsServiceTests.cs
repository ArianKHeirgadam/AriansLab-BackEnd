using Application.DTOs.Analytics;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Services;

namespace AriansLab.ApiTests;

public sealed class AnalyticsServiceTests
{
    [Fact]
    public async Task TrackPageView_NormalizesHashesAndDeduplicates()
    {
        await using var dbContext = CreateDbContext();
        var service = new AnalyticsWriteService(dbContext);
        var visitorId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var request = new TrackPageViewRequestDto
        {
            Path = "https://arianslab.example/products/?campaign=test#pricing",
            VisitorId = visitorId,
            SessionId = sessionId,
            ReferrerHost = " Google.COM. "
        };

        const string userAgent =
            "Mozilla/5.0 (Linux; Android 14; Mobile) AppleWebKit/537.36 Chrome/126.0 Safari/537.36";
        await service.TrackPageViewAsync(request, userAgent);
        await service.TrackPageViewAsync(request, userAgent);

        var pageView = await dbContext.PageViews.SingleAsync();
        Assert.Equal("/products", pageView.Path);
        Assert.Equal("google.com", pageView.ReferrerHost);
        Assert.Equal("mobile", pageView.DeviceType);
        Assert.Equal("Chrome", pageView.Browser);
        Assert.Equal(64, pageView.VisitorIdHash.Length);
        Assert.Equal(64, pageView.SessionIdHash.Length);
        Assert.DoesNotContain(visitorId.ToString("N"), pageView.VisitorIdHash);
        Assert.DoesNotContain(sessionId.ToString("N"), pageView.SessionIdHash);

        await service.TrackPageViewAsync(
            new TrackPageViewRequestDto
            {
                Path = "/",
                VisitorId = Guid.NewGuid(),
                SessionId = Guid.NewGuid()
            },
            "Googlebot/2.1");

        Assert.Equal(1, await dbContext.PageViews.CountAsync());
    }

    [Fact]
    public async Task Dashboard_UsesStoredTrafficAndBusinessData()
    {
        await using var dbContext = CreateDbContext();
        dbContext.PageViews.AddRange(
            CreatePageView("/", "visitor-a", "session-a", "direct", "desktop", "Chrome"),
            CreatePageView("/products", "visitor-b", "session-b", "google.com", "mobile", "Firefox"));
        dbContext.Users.Add(new User
        {
            FullName = "Analytics customer",
            Email = "analytics@example.com",
            NormalizedEmail = "ANALYTICS@EXAMPLE.COM",
            UserName = "analytics_customer",
            NormalizedUserName = "ANALYTICS_CUSTOMER",
            PasswordHash = "test-only",
            Role = UserRole.Customer
        });
        dbContext.Payments.Add(new Payment
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 250_000m,
            Gateway = "test",
            Authority = "test",
            Status = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow
        });
        dbContext.ContactMessages.Add(new ContactMessage
        {
            FullName = "Visitor",
            Email = "visitor@example.com",
            PhoneNumber = "09120000000",
            Subject = "Test",
            Message = "Unread analytics test",
            IsRead = false
        });
        dbContext.SupportTickets.Add(new SupportTicket
        {
            TicketNumber = "T-ANALYTICS",
            UserId = Guid.NewGuid(),
            Title = "Open ticket",
            Description = "Analytics test",
            Status = TicketStatus.Open
        });
        await dbContext.SaveChangesAsync();

        var dashboard = await new AnalyticsReadService(dbContext).GetDashboardAsync(30);

        Assert.Equal(2L, dashboard.Overview.TotalPageViews);
        Assert.Equal(2, dashboard.Overview.PeriodPageViews);
        Assert.Equal(2, dashboard.Overview.UniqueVisitors);
        Assert.Equal(1, dashboard.Overview.TotalUsers);
        Assert.Equal(1, dashboard.Overview.NewRegistrations);
        Assert.Equal(250_000m, dashboard.Overview.PaidRevenue);
        Assert.Equal(1, dashboard.Overview.UnreadMessages);
        Assert.Equal(1, dashboard.Overview.OpenTickets);
        Assert.Equal(2, dashboard.Daily.Sum(item => item.PageViews));
        Assert.Contains(dashboard.TopPages, item => item.Path == "/products");
        Assert.Contains(dashboard.Devices, item => item.Name == "mobile");
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"AnalyticsTests-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static PageView CreatePageView(
        string path,
        string visitorIdHash,
        string sessionIdHash,
        string? referrerHost,
        string deviceType,
        string browser) => new()
    {
        Path = path,
        VisitorIdHash = visitorIdHash.PadRight(64, '0'),
        SessionIdHash = sessionIdHash.PadRight(64, '0'),
        ReferrerHost = referrerHost == "direct" ? null : referrerHost,
        DeviceType = deviceType,
        Browser = browser
    };
}
