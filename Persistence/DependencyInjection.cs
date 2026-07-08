using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Repositories;
using Persistence.Services;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
        });

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IBlogReadService, BlogReadService>();
        services.AddScoped<IBlogAdminCategoryService, BlogAdminCategoryService>();
        services.AddScoped<IBlogAdminPostService, BlogAdminPostService>();
        services.AddScoped<IPortfolioReadService, PortfolioReadService>();
        services.AddScoped<IPricingReadService, PricingReadService>();
        services.AddScoped<IServiceReadService, ServiceReadService>();
        services.AddScoped<IServiceAdminService, ServiceAdminService>();
        services.AddScoped<IPricingAdminService, PricingAdminService>();
        services.AddScoped<IFaqReadService, FaqReadService>();
        services.AddScoped<IFaqAdminService, FaqAdminService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();
        services.AddScoped<IContactMessageAdminService, ContactMessageAdminService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IProjectReadService, ProjectReadService>();
        services.AddScoped<IProjectAdminService, ProjectAdminService>();
        services.AddScoped<IInvoiceReadService, InvoiceReadService>();
        services.AddScoped<IInvoiceAdminService, InvoiceAdminService>();
        services.AddScoped<IPaymentReadService, PaymentReadService>();
        services.AddScoped<IPaymentAdminService, PaymentAdminService>();
        services.AddScoped<ISupportTicketService, SupportTicketService>();
        services.AddScoped<ISupportTicketAdminService, SupportTicketAdminService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationAdminService, NotificationAdminService>();
        services.AddScoped<IProjectFileService, ProjectFileService>();
        services.AddScoped<IProjectFileAdminService, ProjectFileAdminService>();
        services.AddScoped<IFileAttachmentService, FileAttachmentService>();
        services.AddScoped<IFileAttachmentAdminService, FileAttachmentAdminService>();
        services.AddScoped<IHeroSectionReadService, HeroSectionReadService>();
        services.AddScoped<IHeroSectionAdminService, HeroSectionAdminService>();
        services.AddScoped<ISiteSettingReadService, SiteSettingReadService>();
        services.AddScoped<ISiteSettingAdminService, SiteSettingAdminService>();
        services.AddScoped<ISettingReadService, SettingReadService>();
        services.AddScoped<ISettingAdminService, SettingAdminService>();
        services.AddScoped<ISocialMediaReadService, SocialMediaReadService>();
        services.AddScoped<ISocialMediaAdminService, SocialMediaAdminService>();
        services.AddScoped<IEmailTemplateAdminService, EmailTemplateAdminService>();
        services.AddScoped<IActivityLogReadService, ActivityLogReadService>();
        services.AddScoped<IAuditLogReadService, AuditLogReadService>();
        services.AddScoped<IActivityLogWriteService, ActivityLogWriteService>();
        services.AddScoped<IAuditLogWriteService, AuditLogWriteService>();
        services.AddScoped<ICommentReadService, CommentReadService>();
        services.AddScoped<ICommentSubmitService, CommentSubmitService>();
        services.AddScoped<ICommentAdminService, CommentAdminService>();
        services.AddScoped<IBlogCategoryReadService, BlogCategoryReadService>();
        services.AddScoped<IBlogCategoryAdminService, BlogCategoryAdminService>();
        services.AddScoped<ITechnologyReadService, TechnologyReadService>();
        services.AddScoped<ITechnologyAdminService, TechnologyAdminService>();
        return services;
    }
}