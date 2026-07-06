using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PortfolioImageConfiguration : IEntityTypeConfiguration<PortfolioImage>
{
    public void Configure(EntityTypeBuilder<PortfolioImage> builder)
    {
        builder.ToTable("PortfolioImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.IsCover)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Portfolio)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}