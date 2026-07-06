using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context
{
    public sealed class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        #region Authentication

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        #endregion

        #region Website

        public DbSet<Service> Services => Set<Service>();

        public DbSet<ServiceFeature> ServiceFeatures => Set<ServiceFeature>();

        public DbSet<PricingPlan> PricingPlans => Set<PricingPlan>();

        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();

        public DbSet<Portfolio> Portfolios => Set<Portfolio>();

        public DbSet<PortfolioCategory> PortfolioCategories => Set<PortfolioCategory>();

        public DbSet<PortfolioImage> PortfolioImages => Set<PortfolioImage>();

        public DbSet<Technology> Technologies => Set<Technology>();

        public DbSet<PortfolioTechnology> PortfolioTechnologies => Set<PortfolioTechnology>();

        public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();

        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

        public DbSet<Comment> Comments => Set<Comment>();

        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

        public DbSet<FAQ> FAQs => Set<FAQ>();

        public DbSet<HeroSection> HeroSections => Set<HeroSection>();

        //slider

        public DbSet<SocialMedia> SocialMedias => Set<SocialMedia>();

        public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

        #endregion

        #region Customer

        public DbSet<Project> Projects => Set<Project>();

        public DbSet<ProjectFile> ProjectFiles => Set<ProjectFile>();

        public DbSet<Invoice> Invoices => Set<Invoice>();

        public DbSet<Payment> Payments => Set<Payment>();

        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

        public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();

        public DbSet<Notification> Notifications => Set<Notification>();

        #endregion

        #region System

        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

        public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:

                        entry.Entity.CreatedAt = DateTime.UtcNow;

                        break;

                    case EntityState.Modified:

                        entry.Entity.UpdatedAt = DateTime.UtcNow;

                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}
