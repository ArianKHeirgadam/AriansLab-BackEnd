using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PortfolioTechnologyConfiguration : IEntityTypeConfiguration<PortfolioTechnology>
{
    public void Configure(EntityTypeBuilder<PortfolioTechnology> builder)
    {
        builder.ToTable("PortfolioTechnologies");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.PortfolioId, x.TechnologyId })
            .IsUnique();

        builder.HasOne(x => x.Portfolio)
            .WithMany(x => x.Technologies)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Technology)
            .WithMany(x => x.Portfolios)
            .HasForeignKey(x => x.TechnologyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}