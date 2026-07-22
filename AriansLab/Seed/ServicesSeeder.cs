using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace AriansLab.Api.Seed;

public static class ServicesSeeder
{
    private sealed record ServiceSeedItem(
        string LegacyTitle,
        string Title,
        string Slug,
        string ShortDescription,
        string Description,
        int EstimatedDeliveryDays,
        bool IsFeatured,
        int DisplayOrder,
        string Icon,
        IReadOnlyList<string> Features);

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ServicesSeeder");

        var hasAnyServices = await dbContext.Services.AnyAsync();
        var now = DateTime.UtcNow;
        var insertedCount = 0;
        var migratedCount = 0;

        foreach (var item in CreateSeedItems())
        {
            var existingService = await dbContext.Services
                .Include(x => x.Features)
                .FirstOrDefaultAsync(x => x.Slug == item.Slug);

            if (existingService is null)
            {
                // Preserve databases already managed from the admin panel. Default services
                // are created only for a fresh database, matching the old seeder behaviour.
                if (hasAnyServices)
                {
                    continue;
                }

                var service = new Service
                {
                    Id = Guid.NewGuid(),
                    Slug = item.Slug,
                    Thumbnail = string.Empty,
                    CoverImage = string.Empty,
                    CreatedAt = now,
                    IsDeleted = false
                };

                ApplySeedValues(service, item, now, isNew: true);
                await dbContext.Services.AddAsync(service);
                insertedCount++;
                continue;
            }

            if (existingService.Title != item.LegacyTitle)
            {
                continue;
            }

            ApplySeedValues(existingService, item, now, isNew: false);
            migratedCount++;
        }

        if (insertedCount == 0 && migratedCount == 0)
        {
            return;
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Persian services seed completed. Inserted: {InsertedCount}, migrated legacy services: {MigratedCount}",
            insertedCount,
            migratedCount);
    }

    private static void ApplySeedValues(
        Service service,
        ServiceSeedItem item,
        DateTime now,
        bool isNew)
    {
        service.Title = item.Title;
        service.ShortDescription = item.ShortDescription;
        service.Description = item.Description;
        service.EstimatedDeliveryDays = item.EstimatedDeliveryDays;
        service.IsFeatured = item.IsFeatured;
        service.DisplayOrder = item.DisplayOrder;
        service.Icon = item.Icon;
        service.IsActive = true;
        service.IsDeleted = false;

        if (!isNew)
        {
            service.UpdatedAt = now;
        }

        var orderedFeatures = service.Features
            .OrderBy(feature => feature.DisplayOrder)
            .ToList();

        for (var index = 0; index < item.Features.Count; index++)
        {
            if (index < orderedFeatures.Count)
            {
                orderedFeatures[index].Title = item.Features[index];
                orderedFeatures[index].DisplayOrder = index + 1;
                orderedFeatures[index].IsDeleted = false;
                orderedFeatures[index].UpdatedAt = isNew ? null : now;
                continue;
            }

            service.Features.Add(new ServiceFeature
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                Service = service,
                Title = item.Features[index],
                DisplayOrder = index + 1,
                CreatedAt = now,
                IsDeleted = false
            });
        }
    }

    private static IReadOnlyList<ServiceSeedItem> CreateSeedItems()
    {
        return new List<ServiceSeedItem>
        {
            new(
                LegacyTitle: "Web Development",
                Title: "طراحی و توسعه وب‌سایت",
                Slug: "web-development",
                ShortDescription: "طراحی سایت شرکتی و خدماتی سریع، واکنش‌گرا و آماده رشد کسب‌وکار.",
                Description: """
                    وب‌سایت شما باید در چند ثانیه هویت برند، ارزش پیشنهادی و مسیر اقدام را برای مخاطب روشن کند. در این خدمت، ساختار محتوا، تجربه کاربری، طراحی رابط و پیاده‌سازی فنی به‌صورت یکپارچه انجام می‌شود.

                    خروجی بر اساس موبایل و دسکتاپ تست می‌شود، ساختار فنی مناسب سئو دارد و برای اتصال به فرم‌ها، پنل مدیریت و سرویس‌های موردنیاز آماده است. پروژه مرحله‌ای ارائه می‌شود تا پیش از تحویل نهایی امکان بازبینی و اصلاح داشته باشید.
                    """,
                EstimatedDeliveryDays: 14,
                IsFeatured: true,
                DisplayOrder: 1,
                Icon: "globe",
                Features: new[]
                {
                    "طراحی واکنش‌گرا برای موبایل، تبلت و دسکتاپ",
                    "ساختار صفحات متناسب با هدف و مسیر کاربر",
                    "بهینه‌سازی سرعت و آماده‌سازی فنی سئو",
                    "اتصال فرم‌ها و بخش‌های پویا به API"
                }),
            new(
                LegacyTitle: "Backend Development",
                Title: "توسعه بک‌اند و سامانه اختصاصی",
                Slug: "backend-development",
                ShortDescription: "ساخت API امن، پنل مدیریتی و منطق اختصاصی برای فرایندهای واقعی کسب‌وکار.",
                Description: """
                    وقتی نیاز پروژه از یک سایت ساده فراتر می‌رود، بک‌اند باید داده، کاربران، دسترسی‌ها و گردش‌کار را قابل‌اعتماد مدیریت کند. این خدمت برای ساخت API، پنل مدیریتی، اتصال دیتابیس و خودکارسازی فرایندهای اختصاصی ارائه می‌شود.

                    معماری بر اساس اندازه و مسیر رشد محصول انتخاب می‌شود. احراز هویت، کنترل نقش‌ها، اعتبارسنجی داده، ثبت خطا و قابلیت نگهداری از ابتدا جزو طراحی سیستم هستند؛ نه مواردی که بعداً به پروژه اضافه شوند.
                    """,
                EstimatedDeliveryDays: 21,
                IsFeatured: true,
                DisplayOrder: 2,
                Icon: "server",
                Features: new[]
                {
                    "طراحی و توسعه REST API با ASP.NET Core",
                    "طراحی دیتابیس و مدیریت امن اطلاعات",
                    "احراز هویت، نقش‌ها و سطح دسترسی کاربران",
                    "معماری قابل تست و آماده توسعه آینده"
                }),
            new(
                LegacyTitle: "UI/UX Design",
                Title: "طراحی رابط و تجربه کاربری",
                Slug: "ui-ux-design",
                ShortDescription: "طراحی رابط روشن، منسجم و کاربرمحور برای وب‌سایت، داشبورد و محصول دیجیتال.",
                Description: """
                    طراحی خوب فقط انتخاب رنگ و فونت نیست؛ کاربر باید بدون سردرگمی بداند کجا قرار دارد و قدم بعدی چیست. ابتدا ساختار اطلاعات و مسیرهای اصلی بررسی می‌شوند و سپس رابطی متناسب با برند و نوع مخاطب شکل می‌گیرد.

                    صفحات کلیدی، حالت‌های موبایل، فرم‌ها و اجزای تکرارشونده در یک سیستم منسجم طراحی می‌شوند. نتیجه، رابطی است که هم زیباست و هم پیاده‌سازی و توسعه آن برای تیم فنی روشن و قابل مدیریت باقی می‌ماند.
                    """,
                EstimatedDeliveryDays: 10,
                IsFeatured: true,
                DisplayOrder: 3,
                Icon: "palette",
                Features: new[]
                {
                    "طراحی وایرفریم و مسیرهای اصلی کاربر",
                    "طراحی رابط صفحات وب و داشبورد",
                    "ساخت کامپوننت‌ها و زبان بصری یکپارچه",
                    "طراحی نسخه موبایل و حالت‌های تعاملی"
                }),
            new(
                LegacyTitle: "SEO Optimization",
                Title: "سئو و بهینه‌سازی فنی",
                Slug: "seo-optimization",
                ShortDescription: "بهبود ساختار، سرعت و نشانه‌های فنی سایت برای دیده‌شدن بهتر در جست‌وجو.",
                Description: """
                    سئو زمانی نتیجه می‌دهد که موتور جست‌وجو بتواند صفحات را درست پیدا و تحلیل کند و کاربر نیز تجربه سریع و قابل‌اعتمادی داشته باشد. در این خدمت، وضعیت فنی سایت، ساختار محتوا، متادیتا و مشکلات ایندکس بررسی می‌شوند.

                    خروجی شامل اولویت‌بندی مشکلات، اصلاح موارد قابل اجرا و پیشنهاد مسیر ادامه است. تمرکز بر اقدام‌های واقعی و قابل‌اندازه‌گیری است؛ نه وعده رتبه قطعی یا گزارش‌های مبهم.
                    """,
                EstimatedDeliveryDays: 7,
                IsFeatured: false,
                DisplayOrder: 4,
                Icon: "search",
                Features: new[]
                {
                    "بررسی متادیتا و ساختار عنوان صفحات",
                    "ارزیابی ایندکس، نقشه سایت و ریدایرکت‌ها",
                    "بهبود سرعت و معیارهای تجربه صفحه",
                    "پیشنهاد ساختار محتوا و لینک‌سازی داخلی"
                })
        };
    }
}
