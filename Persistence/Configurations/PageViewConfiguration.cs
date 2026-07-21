using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> builder)
    {
        builder.ToTable("PageViews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Path)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.VisitorIdHash)
            .HasMaxLength(64)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.SessionIdHash)
            .HasMaxLength(64)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.ReferrerHost)
            .HasMaxLength(253);

        builder.Property(x => x.DeviceType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Browser)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.Path, x.CreatedAt });
        builder.HasIndex(x => new { x.VisitorIdHash, x.CreatedAt });
        builder.HasIndex(x => new { x.SessionIdHash, x.CreatedAt });
    }
}
