using Application.DTOs.Settings;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SettingAdminService : ISettingAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public SettingAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SettingDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Settings
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SettingDto
            {
                Id = x.Id,
                SiteName = x.SiteName,
                Logo = x.Logo,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                Telegram = x.Telegram,
                Instagram = x.Instagram,
                Linkedin = x.Linkedin,
                WhatsApp = x.WhatsApp,
                FooterDescription = x.FooterDescription,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SettingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Settings
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SettingDto
            {
                Id = x.Id,
                SiteName = x.SiteName,
                Logo = x.Logo,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                Telegram = x.Telegram,
                Instagram = x.Instagram,
                Linkedin = x.Linkedin,
                WhatsApp = x.WhatsApp,
                FooterDescription = x.FooterDescription,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SettingDto> CreateAsync(
        CreateSettingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSetting(
            request.SiteName,
            request.Logo,
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.FooterDescription
        );

        var setting = new Setting
        {
            SiteName = request.SiteName.Trim(),
            Logo = request.Logo.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim(),
            Telegram = NormalizeString(request.Telegram),
            Instagram = NormalizeString(request.Instagram),
            Linkedin = NormalizeString(request.Linkedin),
            WhatsApp = NormalizeString(request.WhatsApp),
            FooterDescription = request.FooterDescription.Trim()
        };

        await _dbContext.Settings.AddAsync(setting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdSetting = await GetByIdAsync(setting.Id, cancellationToken);

        return createdSetting!;
    }

    public async Task<SettingDto?> UpdateAsync(
        Guid id,
        UpdateSettingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateSetting(
            request.SiteName,
            request.Logo,
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.FooterDescription
        );

        var setting = await _dbContext.Settings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (setting is null)
        {
            return null;
        }

        setting.SiteName = request.SiteName.Trim();
        setting.Logo = request.Logo.Trim();
        setting.Email = request.Email.Trim();
        setting.PhoneNumber = request.PhoneNumber.Trim();
        setting.Address = request.Address.Trim();
        setting.Telegram = NormalizeString(request.Telegram);
        setting.Instagram = NormalizeString(request.Instagram);
        setting.Linkedin = NormalizeString(request.Linkedin);
        setting.WhatsApp = NormalizeString(request.WhatsApp);
        setting.FooterDescription = request.FooterDescription.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(setting.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.Settings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (setting is null)
        {
            return false;
        }

        _dbContext.Settings.Remove(setting);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateSetting(
        string siteName,
        string logo,
        string email,
        string phoneNumber,
        string address,
        string footerDescription)
    {
        if (string.IsNullOrWhiteSpace(siteName))
        {
            throw new InvalidOperationException("Site name is required.");
        }

        if (string.IsNullOrWhiteSpace(logo))
        {
            throw new InvalidOperationException("Logo is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new InvalidOperationException("Phone number is required.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException("Address is required.");
        }

        if (string.IsNullOrWhiteSpace(footerDescription))
        {
            throw new InvalidOperationException("Footer description is required.");
        }
    }

    private static string NormalizeString(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }
}