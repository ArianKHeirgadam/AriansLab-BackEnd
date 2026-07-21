using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using System.Reflection;

namespace Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    public DbSet<FAQ> FAQs => Set<FAQ>();

    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

    public DbSet<HeroSection> HeroSections => Set<HeroSection>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<PageView> PageViews => Set<PageView>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();

    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    public DbSet<PortfolioCategory> PortfolioCategories => Set<PortfolioCategory>();

    public DbSet<PortfolioImage> PortfolioImages => Set<PortfolioImage>();

    public DbSet<PortfolioTechnology> PortfolioTechnologies => Set<PortfolioTechnology>();

    public DbSet<PricingPlan> PricingPlans => Set<PricingPlan>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectFile> ProjectFiles => Set<ProjectFile>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<ServiceFeature> ServiceFeatures => Set<ServiceFeature>();

    public DbSet<Setting> Settings => Set<Setting>();

    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    public DbSet<SocialMedia> SocialMedias => Set<SocialMedia>();

    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    public DbSet<Technology> Technologies => Set<Technology>();

    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        ConfigureDecimalProperties(modelBuilder);

        modelBuilder.ApplyAuditableEntityConfiguration();

        modelBuilder.ApplySoftDeleteQueryFilters();
    }

    public override int SaveChanges()
    {
        ApplyAuditing();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureDecimalProperties(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>()
            .Property(x => x.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Invoice>()
            .Property(x => x.FinalAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Invoice>()
            .Property(x => x.TaxAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<PricingPlan>()
            .Property(x => x.Price)
            .HasColumnType("decimal(18,2)");
    }

    private void ApplyAuditing()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = null;
                entry.Entity.IsDeleted = false;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<SoftDeleteEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
            }
        }
    }
}
