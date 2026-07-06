using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PortfolioCategoryConfiguration : IEntityTypeConfiguration<PortfolioCategory>
{
    public void Configure(EntityTypeBuilder<PortfolioCategory> builder)
    {
        builder.ToTable("PortfolioCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasMany(x => x.Portfolios)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.PortfolioCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}