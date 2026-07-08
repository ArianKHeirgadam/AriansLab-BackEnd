using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class SettingReadService : ISettingReadService
{
    private readonly ApplicationDbContext _dbContext;

    public SettingReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SettingDto?> GetCurrentAsync(
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
            .FirstOrDefaultAsync(cancellationToken);
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
}