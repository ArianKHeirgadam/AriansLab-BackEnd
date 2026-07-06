using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("Portfolios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(280)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.ClientName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Thumbnail)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.GithubUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.ShortDescription)
            .HasMaxLength(500);

        builder.Property(x => x.WebsiteUrl)
            .HasMaxLength(1000);

        builder.Property(x => x.IsFeatured)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Portfolios)
            .HasForeignKey(x => x.PortfolioCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Portfolio)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Technologies)
            .WithOne(x => x.Portfolio)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}