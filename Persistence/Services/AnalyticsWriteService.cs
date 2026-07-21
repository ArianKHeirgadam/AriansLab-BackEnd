using Application.DTOs.Analytics;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Security.Cryptography;

namespace Persistence.Services;

public sealed class AnalyticsWriteService : IAnalyticsWriteService
{
    private static readonly string[] BotMarkers =
    [
        "bot", "crawler", "spider", "slurp", "bingpreview", "headless", "lighthouse"
    ];

    private readonly ApplicationDbContext _dbContext;

    public AnalyticsWriteService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task TrackPageViewAsync(
        TrackPageViewRequestDto request,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        if (request.VisitorId == Guid.Empty || request.SessionId == Guid.Empty || IsBot(userAgent))
        {
            return;
        }

        var path = NormalizePath(request.Path);
        if (path is null || IsPrivatePath(path))
        {
            return;
        }

        var visitorIdHash = HashIdentifier(request.VisitorId);
        var sessionIdHash = HashIdentifier(request.SessionId);
        var duplicateCutoff = DateTime.UtcNow.AddSeconds(-10);

        var isDuplicate = await _dbContext.PageViews.AnyAsync(
            item => item.SessionIdHash == sessionIdHash &&
                    item.Path == path &&
                    item.CreatedAt >= duplicateCutoff,
            cancellationToken);

        if (isDuplicate)
        {
            return;
        }

        _dbContext.PageViews.Add(new PageView
        {
            Path = path,
            VisitorIdHash = visitorIdHash,
            SessionIdHash = sessionIdHash,
            ReferrerHost = NormalizeReferrerHost(request.ReferrerHost),
            DeviceType = DetectDeviceType(userAgent),
            Browser = DetectBrowser(userAgent)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizePath(string? value)
    {
        var path = value?.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
        {
            path = absoluteUri.AbsolutePath;
        }

        var suffixIndex = path.IndexOfAny(['?', '#']);
        if (suffixIndex >= 0)
        {
            path = path[..suffixIndex];
        }

        if (!path.StartsWith('/') || path.Length > 300 || path.Any(char.IsControl))
        {
            return null;
        }

        return path.Length > 1 ? path.TrimEnd('/') : path;
    }

    private static bool IsPrivatePath(string path) =>
        path.Equals("/login", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/register", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/dashboard", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizeReferrerHost(string? value)
    {
        var host = value?.Trim().Trim('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(host) ||
            host.Length > 253 ||
            Uri.CheckHostName(host) == UriHostNameType.Unknown)
        {
            return null;
        }

        return host;
    }

    private static string HashIdentifier(Guid value) =>
        Convert.ToHexString(SHA256.HashData(value.ToByteArray())).ToLowerInvariant();

    private static bool IsBot(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return false;
        }

        return BotMarkers.Any(marker =>
            userAgent.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static string DetectDeviceType(string? userAgent)
    {
        var value = userAgent ?? string.Empty;
        if (value.Contains("ipad", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("tablet", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("kindle", StringComparison.OrdinalIgnoreCase))
        {
            return "tablet";
        }

        if (value.Contains("mobile", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("iphone", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("android", StringComparison.OrdinalIgnoreCase))
        {
            return "mobile";
        }

        return "desktop";
    }

    private static string DetectBrowser(string? userAgent)
    {
        var value = userAgent ?? string.Empty;
        if (value.Contains("Edg/", StringComparison.OrdinalIgnoreCase)) return "Edge";
        if (value.Contains("OPR/", StringComparison.OrdinalIgnoreCase)) return "Opera";
        if (value.Contains("Firefox/", StringComparison.OrdinalIgnoreCase)) return "Firefox";
        if (value.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)) return "Chrome";
        if (value.Contains("Safari/", StringComparison.OrdinalIgnoreCase)) return "Safari";
        return "Other";
    }
}
