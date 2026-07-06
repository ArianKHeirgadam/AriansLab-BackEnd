using Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyAuditableEntityConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                continue;
            }

            modelBuilder.Entity(clrType)
                .Property(nameof(AuditableEntity.CreatedAt))
                .IsRequired();

            modelBuilder.Entity(clrType)
                .Property(nameof(AuditableEntity.IsDeleted))
                .HasDefaultValue(false);
        }
    }

    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "entity");
            var property = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }
}