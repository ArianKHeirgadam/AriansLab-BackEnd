using Application.DTOs.Settings;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SiteSettingAdminService : ISiteSettingAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public SiteSettingAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SiteSettingDto>> GetAllAsync(
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
            .ToListAsync(cancellationToken);
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

    public async Task<SiteSettingDto> CreateAsync(
        CreateSiteSettingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSiteSetting(
            request.SiteName,
            request.Email,
            request.Phone,
            request.Address,
            request.FooterText,
            request.Copyright,
            request.MetaTitle,
            request.MetaDescription,
            request.MetaKeywords
        );

        var siteSetting = new SiteSetting
        {
            SiteName = request.SiteName.Trim(),
            Logo = NormalizeNullableString(request.Logo),
            DarkLogo = NormalizeNullableString(request.DarkLogo),
            Favicon = NormalizeNullableString(request.Favicon),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            FooterText = request.FooterText.Trim(),
            Copyright = request.Copyright.Trim(),
            GoogleMap = NormalizeNullableString(request.GoogleMap),
            GoogleAnalytics = NormalizeNullableString(request.GoogleAnalytics),
            MetaTitle = request.MetaTitle.Trim(),
            MetaDescription = request.MetaDescription.Trim(),
            MetaKeywords = request.MetaKeywords.Trim()
        };

        await _dbContext.SiteSettings.AddAsync(siteSetting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdSiteSetting = await GetByIdAsync(
            siteSetting.Id,
            cancellationToken
        );

        return createdSiteSetting!;
    }

    public async Task<SiteSettingDto?> UpdateAsync(
        Guid id,
        UpdateSiteSettingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSiteSetting(
            request.SiteName,
            request.Email,
            request.Phone,
            request.Address,
            request.FooterText,
            request.Copyright,
            request.MetaTitle,
            request.MetaDescription,
            request.MetaKeywords
        );

        var siteSetting = await _dbContext.SiteSettings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (siteSetting is null)
        {
            return null;
        }

        siteSetting.SiteName = request.SiteName.Trim();
        siteSetting.Logo = NormalizeNullableString(request.Logo);
        siteSetting.DarkLogo = NormalizeNullableString(request.DarkLogo);
        siteSetting.Favicon = NormalizeNullableString(request.Favicon);
        siteSetting.Email = request.Email.Trim();
        siteSetting.Phone = request.Phone.Trim();
        siteSetting.Address = request.Address.Trim();
        siteSetting.FooterText = request.FooterText.Trim();
        siteSetting.Copyright = request.Copyright.Trim();
        siteSetting.GoogleMap = NormalizeNullableString(request.GoogleMap);
        siteSetting.GoogleAnalytics = NormalizeNullableString(request.GoogleAnalytics);
        siteSetting.MetaTitle = request.MetaTitle.Trim();
        siteSetting.MetaDescription = request.MetaDescription.Trim();
        siteSetting.MetaKeywords = request.MetaKeywords.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(siteSetting.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var siteSetting = await _dbContext.SiteSettings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (siteSetting is null)
        {
            return false;
        }

        _dbContext.SiteSettings.Remove(siteSetting);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateSiteSetting(
        string siteName,
        string email,
        string phone,
        string address,
        string footerText,
        string copyright,
        string metaTitle,
        string metaDescription,
        string metaKeywords)
    {
        if (string.IsNullOrWhiteSpace(siteName))
        {
            throw new InvalidOperationException("Site name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new InvalidOperationException("Phone is required.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException("Address is required.");
        }

        if (string.IsNullOrWhiteSpace(footerText))
        {
            throw new InvalidOperationException("Footer text is required.");
        }

        if (string.IsNullOrWhiteSpace(copyright))
        {
            throw new InvalidOperationException("Copyright is required.");
        }

        if (string.IsNullOrWhiteSpace(metaTitle))
        {
            throw new InvalidOperationException("Meta title is required.");
        }

        if (string.IsNullOrWhiteSpace(metaDescription))
        {
            throw new InvalidOperationException("Meta description is required.");
        }

        if (string.IsNullOrWhiteSpace(metaKeywords))
        {
            throw new InvalidOperationException("Meta keywords are required.");
        }
    }

    private static string? NormalizeNullableString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}