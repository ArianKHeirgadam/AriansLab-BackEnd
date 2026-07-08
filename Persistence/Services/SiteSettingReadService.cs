using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SiteSettingReadService : ISiteSettingReadService
{
    private readonly ApplicationDbContext _dbContext;

    public SiteSettingReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SiteSettingDto?> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SiteSettings
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SiteSettingDto
            {
                Id = x.Id,
                SiteName = x.SiteName,
                Logo = x.Logo,
                DarkLogo = x.DarkLogo,
                Favicon = x.Favicon,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                FooterText = x.FooterText,
                Copyright = x.Copyright,
                GoogleMap = x.GoogleMap,
                GoogleAnalytics = x.GoogleAnalytics,
                MetaTitle = x.MetaTitle,
                MetaDescription = x.MetaDescription,
                MetaKeywords = x.MetaKeywords,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SiteSettingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SiteSettings
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SiteSettingDto
            {
                Id = x.Id,
                SiteName = x.SiteName,
                Logo = x.Logo,
                DarkLogo = x.DarkLogo,
                Favicon = x.Favicon,
                Email = x.Email,
                Phone = x.Phone,
                Address = x.Address,
                FooterText = x.FooterText,
                Copyright = x.Copyright,
                GoogleMap = x.GoogleMap,
                GoogleAnalytics = x.GoogleAnalytics,
                MetaTitle = x.MetaTitle,
                MetaDescription = x.MetaDescription,
                MetaKeywords = x.MetaKeywords,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}