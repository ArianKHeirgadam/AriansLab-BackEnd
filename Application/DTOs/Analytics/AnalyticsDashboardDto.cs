namespace Application.DTOs.Analytics;

public sealed class AnalyticsDashboardDto
{
    public int Days { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public AnalyticsOverviewDto Overview { get; set; } = new();

    public List<AnalyticsDailyPointDto> Daily { get; set; } = [];

    public List<AnalyticsPageDto> TopPages { get; set; } = [];

    public List<AnalyticsBreakdownDto> TrafficSources { get; set; } = [];

    public List<AnalyticsBreakdownDto> Devices { get; set; } = [];

    public List<AnalyticsBreakdownDto> Browsers { get; set; } = [];
}

public sealed class AnalyticsOverviewDto
{
    public long TotalPageViews { get; set; }

    public long PeriodPageViews { get; set; }

    public int UniqueVisitors { get; set; }

    public int TotalUsers { get; set; }

    public int NewRegistrations { get; set; }

    public int TotalProjects { get; set; }

    public decimal PaidRevenue { get; set; }

    public int UnreadMessages { get; set; }

    public int OpenTickets { get; set; }

    public double PageViewsChangePercent { get; set; }

    public double UniqueVisitorsChangePercent { get; set; }

    public double RegistrationsChangePercent { get; set; }

    public double RevenueChangePercent { get; set; }
}

public sealed class AnalyticsDailyPointDto
{
    public string Date { get; set; } = string.Empty;

    public int PageViews { get; set; }

    public int UniqueVisitors { get; set; }

    public int Registrations { get; set; }

    public decimal PaidRevenue { get; set; }
}

public sealed class AnalyticsPageDto
{
    public string Path { get; set; } = string.Empty;

    public int PageViews { get; set; }

    public int UniqueVisitors { get; set; }
}

public sealed class AnalyticsBreakdownDto
{
    public string Name { get; set; } = string.Empty;

    public int Count { get; set; }

    public double Percentage { get; set; }
}
