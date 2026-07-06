using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Gateway)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Authority)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.RefId)
            .HasMaxLength(200);

        builder.Property(x => x.CardPan)
            .HasMaxLength(30);

        builder.Property(x => x.TrackingCode)
            .HasMaxLength(100);

        builder.Property(x => x.GatewayResponse)
            .HasMaxLength(4000);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}