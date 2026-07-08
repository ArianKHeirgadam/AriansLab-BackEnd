using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace AriansLab.Api.Seed;

public static class PricingSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var hasPricingPlans = await dbContext.PricingPlans.AnyAsync();

        if (hasPricingPlans)
        {
            return;
        }

        var starterPlan = new PricingPlan
        {
            Title = "Starter",
            Description = "Perfect for simple websites, landing pages, and small business presence.",
            Price = 4900000,
            Duration = 30,
            DeliveryDays = 7,
            IsPopular = false,
            DisplayOrder = 1,
            IsActive = true,
            Features = new List<PlanFeature>
            {
                new() { Feature = "Responsive website design" },
                new() { Feature = "Up to 5 pages" },
                new() { Feature = "Basic SEO setup" },
                new() { Feature = "Contact form integration" },
                new() { Feature = "1 month technical support" }
            }
        };

        var professionalPlan = new PricingPlan
        {
            Title = "Professional",
            Description = "Best choice for growing brands that need a complete corporate website.",
            Price = 12900000,
            Duration = 60,
            DeliveryDays = 14,
            IsPopular = true,
            DisplayOrder = 2,
            IsActive = true,
            Features = new List<PlanFeature>
            {
                new() { Feature = "Custom corporate website" },
                new() { Feature = "Admin panel" },
                new() { Feature = "Blog module" },
                new() { Feature = "Portfolio module" },
                new() { Feature = "API integration" },
                new() { Feature = "3 months technical support" }
            }
        };

        var enterprisePlan = new PricingPlan
        {
            Title = "Enterprise",
            Description = "Advanced solution for custom platforms, dashboards, and business systems.",
            Price = 24900000,
            Duration = 90,
            DeliveryDays = 30,
            IsPopular = false,
            DisplayOrder = 3,
            IsActive = true,
            Features = new List<PlanFeature>
            {
                new() { Feature = "Custom backend architecture" },
                new() { Feature = "Advanced admin dashboard" },
                new() { Feature = "Role-based access control" },
                new() { Feature = "Reporting system" },
                new() { Feature = "Database design" },
                new() { Feature = "Priority support" }
            }
        };

        await dbContext.PricingPlans.AddRangeAsync(
            starterPlan,
            professionalPlan,
            enterprisePlan
        );

        await dbContext.SaveChangesAsync();
    }
}