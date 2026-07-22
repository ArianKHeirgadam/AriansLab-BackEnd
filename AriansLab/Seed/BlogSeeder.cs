using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace AriansLab.Api.Seed;

public static class BlogSeeder
{
    private sealed record BlogSeedItem(
        string Title,
        string Slug,
        string Excerpt,
        string Content,
        string CoverImage,
        int ReadTime,
        string SeoTitle,
        string SeoDescription,
        string Keywords,
        string CategorySlug,
        int PublishedDaysAgo,
        string? LegacySlug = null,
        string? LegacyTitle = null);

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BlogSeeder");

        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@arianslab.com";
        var normalizedAdminEmail = adminEmail.Trim().ToUpperInvariant();

        var admin = await dbContext.Users
            .FirstOrDefaultAsync(x =>
                x.NormalizedEmail == normalizedAdminEmail &&
                x.Role == UserRole.Admin);

        if (admin is null)
        {
            logger.LogWarning("Blog seed skipped because admin user was not found.");
            return;
        }

        var now = DateTime.UtcNow;
        var categories = new Dictionary<string, BlogCategory>(StringComparer.OrdinalIgnoreCase)
        {
            ["technology"] = await GetOrCreateCategoryAsync(
                dbContext,
                "technology",
                "توسعه وب",
                "Technology",
                now),
            ["business"] = await GetOrCreateCategoryAsync(
                dbContext,
                "business",
                "کسب‌وکار دیجیتال",
                "Business",
                now),
            ["security"] = await GetOrCreateCategoryAsync(
                dbContext,
                "security",
                "امنیت وب",
                null,
                now)
        };

        await dbContext.SaveChangesAsync();

        var seedItems = CreateSeedItems();
        var insertedCount = 0;
        var migratedCount = 0;

        foreach (var item in seedItems)
        {
            var existingPost = await dbContext.BlogPosts
                .FirstOrDefaultAsync(x => x.Slug == item.Slug);

            if (existingPost is not null)
            {
                // A post already using the final slug may have been edited in the admin panel.
                // Keep that user-managed content intact on future application restarts.
                continue;
            }

            BlogPost? legacyPost = null;
            if (!string.IsNullOrWhiteSpace(item.LegacySlug))
            {
                legacyPost = await dbContext.BlogPosts
                    .FirstOrDefaultAsync(x => x.Slug == item.LegacySlug);
            }

            if (legacyPost is not null && legacyPost.Title == item.LegacyTitle)
            {
                ApplySeedValues(
                    legacyPost,
                    item,
                    categories[item.CategorySlug],
                    admin.Id,
                    now,
                    isNew: false);
                migratedCount++;
                continue;
            }

            var post = new BlogPost
            {
                Id = Guid.NewGuid(),
                CreatedAt = now,
                IsDeleted = false
            };

            ApplySeedValues(
                post,
                item,
                categories[item.CategorySlug],
                admin.Id,
                now,
                isNew: true);
            await dbContext.BlogPosts.AddAsync(post);
            insertedCount++;
        }

        if (insertedCount == 0 && migratedCount == 0)
        {
            return;
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Persian blog seed completed. Inserted: {InsertedCount}, migrated legacy posts: {MigratedCount}",
            insertedCount,
            migratedCount);
    }

    private static async Task<BlogCategory> GetOrCreateCategoryAsync(
        ApplicationDbContext dbContext,
        string slug,
        string persianName,
        string? legacyName,
        DateTime now)
    {
        var category = await dbContext.BlogCategories
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (category is null)
        {
            category = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Name = persianName,
                Slug = slug,
                CreatedAt = now,
                IsDeleted = false
            };

            await dbContext.BlogCategories.AddAsync(category);
            return category;
        }

        if (!string.IsNullOrWhiteSpace(legacyName) && category.Name == legacyName)
        {
            category.Name = persianName;
            category.UpdatedAt = now;
        }

        return category;
    }

    private static void ApplySeedValues(
        BlogPost post,
        BlogSeedItem item,
        BlogCategory category,
        Guid authorId,
        DateTime now,
        bool isNew)
    {
        post.Title = item.Title;
        post.Slug = item.Slug;
        post.Excerpt = item.Excerpt;
        post.Content = item.Content;
        post.CoverImage = item.CoverImage;
        post.ReadTime = item.ReadTime;
        post.IsPublished = true;
        post.PublishedAt = now.AddDays(-item.PublishedDaysAgo);
        post.SeoTitle = item.SeoTitle;
        post.SeoDescription = item.SeoDescription;
        post.Keywords = item.Keywords;
        post.CategoryId = category.Id;
        post.AuthorId = authorId;
        post.IsDeleted = false;

        if (!isNew)
        {
            post.UpdatedAt = now;
        }
    }

    private static IReadOnlyList<BlogSeedItem> CreateSeedItems()
    {
        return new List<BlogSeedItem>
        {
            new(
                Title: "SSL چیست و چرا هر وب‌سایت حرفه‌ای به آن نیاز دارد؟",
                Slug: "ssl-certificate-guide",
                Excerpt: "راهنمای ساده و کاربردی گواهی SSL، قفل مرورگر، HTTPS و تأثیر آن بر امنیت، اعتماد کاربران و سئوی سایت.",
                Content: """
                    SSL چیست؟

                    SSL فناوری‌ای است که ارتباط میان مرورگر کاربر و سرور سایت را رمزنگاری می‌کند. وقتی یک وب‌سایت گواهی معتبر داشته باشد، آدرس آن با HTTPS باز می‌شود و مرورگر علامت اتصال امن را نمایش می‌دهد.

                    چرا SSL مهم است؟

                    اطلاعاتی مثل رمز عبور، شماره تماس و داده‌های فرم بدون رمزنگاری می‌توانند در مسیر انتقال دیده یا دست‌کاری شوند. SSL این مسیر را امن‌تر می‌کند و احتمال حمله‌های واسطه را کاهش می‌دهد.

                    مزایای اصلی SSL

                    • افزایش اعتماد کاربر هنگام ثبت‌نام یا خرید
                    • محافظت از اطلاعات در حال انتقال
                    • جلوگیری از نمایش هشدار Not Secure در مرورگر
                    • فراهم‌کردن یکی از پیش‌نیازهای فنی سئو
                    • امکان استفاده مطمئن‌تر از کوکی‌های امن و درگاه پرداخت

                    آیا SSL رایگان کافی است؟

                    برای بیشتر وب‌سایت‌های شخصی و شرکتی، گواهی رایگان معتبر مانند Let's Encrypt کافی است؛ به شرطی که تمدید خودکار، پیکربندی HTTPS و انتقال همه آدرس‌های HTTP به HTTPS درست انجام شود. فروشگاه‌ها و سازمان‌های خاص ممکن است به گواهی‌های سازمانی یا پوشش چند زیردامنه نیاز داشته باشند.

                    جمع‌بندی

                    SSL یک قابلیت تزئینی نیست؛ حداقل استاندارد امنیتی برای هر سایت عمومی است. در پروژه‌های آرین پژوهش، فعال‌سازی HTTPS، تنظیم ریدایرکت و بررسی محتوای ناامن بخشی از آماده‌سازی انتشار محسوب می‌شود.
                    """,
                CoverImage: "https://images.pexels.com/photos/60504/security-protection-anti-virus-software-60504.jpeg?auto=compress&cs=tinysrgb&w=1400",
                ReadTime: 7,
                SeoTitle: "SSL چیست؟ راهنمای کامل HTTPS و امنیت وب‌سایت",
                SeoDescription: "SSL و HTTPS را ساده بشناسید و ببینید چرا گواهی امنیتی برای اعتماد کاربران، حفاظت داده و سئوی سایت ضروری است.",
                Keywords: "SSL, HTTPS, امنیت سایت, گواهی امنیتی, سئو",
                CategorySlug: "security",
                PublishedDaysAgo: 0,
                LegacySlug: "getting-started-with-arianslab",
                LegacyTitle: "Getting Started with AriansLab"),
            new(
                Title: "React در میان ابزارهای فرانت‌اند چه جایگاهی دارد؟",
                Slug: "react-frontend-ranking",
                Excerpt: "بررسی جایگاه React در توسعه فرانت‌اند، مزایا، محدودیت‌ها و پروژه‌هایی که استفاده از آن برایشان منطقی است.",
                Content: """
                    React دقیقاً چیست؟

                    React یک کتابخانه جاوااسکریپت برای ساخت رابط کاربری است. این ابزار با تقسیم رابط به کامپوننت‌های کوچک کمک می‌کند بخش‌های مختلف یک محصول مستقل، قابل استفاده مجدد و قابل نگهداری باشند.

                    چرا React محبوب شد؟

                    اکوسیستم بزرگ، جامعه فعال، ابزارهای توسعه قدرتمند و امکان استفاده در پروژه‌های کوچک تا سامانه‌های بزرگ باعث شده React یکی از انتخاب‌های اصلی تیم‌های فرانت‌اند باشد. Next.js نیز با اضافه‌کردن مسیریابی، رندر سمت سرور و امکانات بهینه‌سازی، استفاده از React را برای پروژه‌های واقعی ساده‌تر می‌کند.

                    نقاط قوت React

                    • ساخت رابط‌های تعاملی و داده‌محور
                    • استفاده مجدد از کامپوننت‌ها
                    • بازار کار و جامعه آموزشی گسترده
                    • هماهنگی مناسب با TypeScript
                    • امکان توسعه وب‌اپلیکیشن‌های بزرگ و مرحله‌ای

                    محدودیت‌ها

                    React به‌تنهایی یک چارچوب کامل نیست و برای مسیریابی، مدیریت داده و معماری پروژه باید تصمیم‌های دیگری هم گرفته شود. در یک سایت معرفی بسیار ساده، استفاده از React ممکن است پیچیدگی غیرضروری ایجاد کند؛ اما برای پنل‌ها، فروشگاه‌ها و محصولات در حال رشد مزیت آن کاملاً دیده می‌شود.

                    نتیجه

                    React لزوماً بهترین انتخاب برای هر صفحه نیست، اما برای رابط‌های تعاملی و محصولاتی که قرار است توسعه پیدا کنند یکی از مطمئن‌ترین گزینه‌های بازار است.
                    """,
                CoverImage: "https://images.pexels.com/photos/11035471/pexels-photo-11035471.jpeg?auto=compress&cs=tinysrgb&w=1400",
                ReadTime: 8,
                SeoTitle: "جایگاه React در فرانت‌اند؛ مزایا و محدودیت‌ها",
                SeoDescription: "React را از نظر کاربرد، مزایا، محدودیت‌ها و تناسب با پروژه‌های مدرن فرانت‌اند بررسی می‌کنیم.",
                Keywords: "React, فرانت‌اند, Next.js, JavaScript, TypeScript",
                CategorySlug: "technology",
                PublishedDaysAgo: 1),
            new(
                Title: "ASP.NET Core در میان فریم‌ورک‌های بک‌اند چه جایگاهی دارد؟",
                Slug: "aspnet-core-backend-ranking",
                Excerpt: "نگاهی واقع‌بینانه به سرعت، امنیت، معماری و کاربرد ASP.NET Core در ساخت API و سامانه‌های حرفه‌ای.",
                Content: """
                    ASP.NET Core چیست؟

                    ASP.NET Core چارچوب متن‌باز و چندسکویی مایکروسافت برای ساخت API، وب‌سایت و سرویس‌های سازمانی است. این فریم‌ورک روی ویندوز و لینوکس اجرا می‌شود و برای پروژه‌های کوچک تا سامانه‌های پرترافیک قابل استفاده است.

                    جایگاه آن در بک‌اند مدرن

                    در مقایسه با Node.js، Django، Laravel و Spring Boot، انتخاب نهایی باید بر اساس تیم، اندازه محصول و نیازهای فنی انجام شود. ASP.NET Core به‌خصوص در پروژه‌هایی که ساختار قوی، کنترل دسترسی دقیق، پردازش سریع و نگهداری بلندمدت مهم است جایگاه بسیار خوبی دارد.

                    مزایای اصلی

                    • عملکرد بالا و مصرف منطقی منابع
                    • پشتیبانی قوی از Type Safety در زبان C#
                    • ابزارهای داخلی برای Dependency Injection، Logging و Configuration
                    • یکپارچگی مناسب با Entity Framework Core
                    • امکانات کامل احراز هویت و کنترل دسترسی
                    • پشتیبانی بلندمدت و مستندات رسمی دقیق

                    چه زمانی انتخاب مناسبی است؟

                    برای پنل‌های مدیریتی، فروشگاه‌ها، سامانه‌های مالی، APIهای موبایل و محصولاتی که چند نقش کاربری و گردش‌کار جدی دارند، ASP.NET Core انتخابی مطمئن است. برای یک نمونه بسیار کوچک، چارچوب‌های سبک‌تر هم می‌توانند سریع‌تر شروع شوند.

                    جمع‌بندی

                    قدرت اصلی ASP.NET Core فقط سرعت نیست؛ ترکیب عملکرد، ابزارهای مهندسی و قابلیت نگهداری آن را به گزینه‌ای جدی برای بک‌اند حرفه‌ای تبدیل می‌کند.
                    """,
                CoverImage: "https://images.pexels.com/photos/546819/pexels-photo-546819.jpeg?auto=compress&cs=tinysrgb&w=1400",
                ReadTime: 9,
                SeoTitle: "جایگاه ASP.NET Core در میان فریم‌ورک‌های بک‌اند",
                SeoDescription: "مزایا، محدودیت‌ها و کاربردهای ASP.NET Core را برای ساخت API و بک‌اند حرفه‌ای بررسی می‌کنیم.",
                Keywords: "ASP.NET Core, بک‌اند, C#, API, Clean Architecture",
                CategorySlug: "technology",
                PublishedDaysAgo: 2,
                LegacySlug: "why-custom-jwt-authentication-matters",
                LegacyTitle: "Why Custom JWT Authentication Matters"),
            new(
                Title: "مقایسه HTML و CSS با React؛ تفاوت ابزار و فریم‌ورک",
                Slug: "html-css-vs-react",
                Excerpt: "HTML و CSS رقیب React نیستند؛ در این مقاله تفاوت نقش آن‌ها و انتخاب درست برای سایت ساده یا وب‌اپلیکیشن را توضیح می‌دهیم.",
                Content: """
                    یک سوءتفاهم رایج

                    HTML ساختار صفحه را تعریف می‌کند و CSS ظاهر آن را می‌سازد. React ابزاری برای مدیریت رابط‌های پویا و کامپوننتی است و در نهایت همان HTML و CSS را در مرورگر تولید و مدیریت می‌کند. بنابراین این سه ابزار جایگزین مستقیم یکدیگر نیستند.

                    چه زمانی HTML و CSS ساده کافی است؟

                    اگر پروژه چند صفحه ثابت، تعامل کم و محتوای محدود دارد، ساختار ساده می‌تواند سریع‌تر، ارزان‌تر و سبک‌تر باشد. لندینگ موقت، رزومه ساده یا یک صفحه اطلاع‌رسانی نمونه‌های خوبی هستند.

                    چه زمانی React ارزش ایجاد می‌کند؟

                    وقتی رابط شامل فرم‌های چندمرحله‌ای، داشبورد، فیلتر، سبد خرید، داده زنده یا کامپوننت‌های تکرارشونده باشد، React نگهداری و توسعه را منظم‌تر می‌کند. با بزرگ‌شدن پروژه، این ساختار جلوی تکرار و آشفتگی کد را می‌گیرد.

                    معیار تصمیم‌گیری

                    • میزان تعامل کاربر
                    • تعداد صفحات و کامپوننت‌های مشترک
                    • برنامه توسعه آینده
                    • نیاز به سئو و رندر سمت سرور
                    • بودجه و زمان تحویل

                    نتیجه

                    انتخاب فناوری باید از نیاز پروژه شروع شود، نه از محبوبیت ابزار. React برای محصول تعاملی و در حال رشد عالی است؛ HTML و CSS ساده هم برای پروژه محدود می‌تواند دقیقاً انتخاب درست باشد.
                    """,
                CoverImage: "https://images.pexels.com/photos/270404/pexels-photo-270404.jpeg?auto=compress&cs=tinysrgb&w=1400",
                ReadTime: 7,
                SeoTitle: "مقایسه HTML و CSS با React؛ کدام مناسب پروژه شماست؟",
                SeoDescription: "تفاوت نقش HTML، CSS و React و معیار انتخاب فناوری مناسب برای سایت ساده یا وب‌اپلیکیشن را بخوانید.",
                Keywords: "HTML, CSS, React, طراحی سایت, فرانت‌اند",
                CategorySlug: "technology",
                PublishedDaysAgo: 3),
            new(
                Title: "چک‌لیست سئوی فنی پیش از انتشار سایت",
                Slug: "technical-seo-checklist",
                Excerpt: "مهم‌ترین بررسی‌های سئوی فنی از عنوان صفحات و نقشه سایت تا سرعت، موبایل و داده‌های ساختاریافته.",
                Content: """
                    سئوی فنی از کجا شروع می‌شود؟

                    پیش از تولید محتوای زیاد، موتور جست‌وجو باید بتواند صفحات را پیدا، دریافت و درست تحلیل کند. سئوی فنی یعنی آماده‌کردن همین زیرساخت.

                    چک‌لیست پیش از انتشار

                    • هر صفحه عنوان و توضیحات یکتای مرتبط داشته باشد
                    • فقط یک H1 روشن در هر صفحه استفاده شود
                    • آدرس‌ها کوتاه، خوانا و پایدار باشند
                    • فایل robots.txt و نقشه سایت بررسی شوند
                    • صفحات خطا و ریدایرکت‌ها درست کار کنند
                    • تصاویر اندازه مناسب و متن جایگزین داشته باشند
                    • نسخه موبایل بدون اسکرول افقی و به‌هم‌ریختگی باشد
                    • HTTPS در تمام صفحات فعال باشد
                    • معیارهای اصلی سرعت و Core Web Vitals بررسی شوند

                    محتوا و ساختار

                    عنوان‌ها باید سلسله‌مراتب منطقی داشته باشند و لینک‌های داخلی کاربر را به صفحات مرتبط هدایت کنند. داده ساختاریافته در صفحات مناسب می‌تواند درک موتور جست‌وجو از نوع محتوا را بهتر کند.

                    بعد از انتشار

                    سایت را در Google Search Console ثبت کنید، خطاهای ایندکس را ببینید و عملکرد صفحات مهم را در موبایل واقعی بررسی کنید. سئو یک تنظیم یک‌باره نیست و به پایش مداوم نیاز دارد.
                    """,
                CoverImage: "https://images.pexels.com/photos/270637/pexels-photo-270637.jpeg?auto=compress&cs=tinysrgb&w=1400",
                ReadTime: 8,
                SeoTitle: "چک‌لیست کامل سئوی فنی پیش از انتشار سایت",
                SeoDescription: "چک‌لیست کاربردی سئوی فنی برای عنوان‌ها، نقشه سایت، سرعت، موبایل، HTTPS و ایندکس صحیح صفحات.",
                Keywords: "SEO, سئوی فنی, سرعت سایت, Google Search Console, Core Web Vitals",
                CategorySlug: "technology",
                PublishedDaysAgo: 4),
            new(
                Title: "راهنمای راه‌اندازی فروشگاه اینترنتی؛ از ایده تا اولین سفارش",
                Slug: "online-store-launch-guide",
                Excerpt: "برای ساخت فروشگاه آنلاین به چه صفحات، امکانات و تصمیم‌هایی نیاز دارید و چگونه مسیر خرید را ساده نگه دارید؟",
                Content: """
                    قبل از طراحی فروشگاه

                    ابتدا محصول، روش ارسال، موجودی، محدوده فروش و شیوه پرداخت را مشخص کنید. بسیاری از مشکلات فروشگاه نه از ظاهر، بلکه از تصمیم‌های نامشخص در عملیات کسب‌وکار ایجاد می‌شوند.

                    صفحات ضروری

                    • صفحه دسته‌بندی و جست‌وجوی محصول
                    • صفحه محصول با تصویر، مشخصات و موجودی روشن
                    • سبد خرید و تسویه حساب کوتاه
                    • قوانین ارسال، مرجوعی و حریم خصوصی
                    • صفحه پیگیری سفارش و راه ارتباط با پشتیبانی

                    تجربه خرید خوب

                    قیمت نهایی و هزینه ارسال باید قبل از پرداخت شفاف باشند. فرم‌های طولانی، اجبار به ساخت حساب و مراحل مبهم باعث رهاشدن سبد خرید می‌شوند. در موبایل، دکمه خرید و اطلاعات مهم باید به‌راحتی در دسترس باشند.

                    بخش مدیریتی

                    مدیریت محصول، سفارش، پرداخت، مشتری، تخفیف و گزارش فروش باید از یک پنل روشن انجام شود. سطح دسترسی مدیران و ثبت سابقه تغییرات نیز برای فروشگاه‌های جدی اهمیت دارد.

                    شروع مرحله‌ای

                    لازم نیست همه امکانات از روز اول ساخته شوند. نسخه اول را با مسیر خرید کامل و امکانات ضروری منتشر کنید، رفتار کاربران را بسنجید و قابلیت‌های بعدی را بر اساس داده واقعی اضافه کنید.
                    """,
                CoverImage: "https://images.pexels.com/photos/5632402/pexels-photo-5632402.jpeg?auto=compress&cs=tinysrgb&w=1400",
                ReadTime: 9,
                SeoTitle: "راهنمای راه‌اندازی فروشگاه اینترنتی از ایده تا سفارش",
                SeoDescription: "صفحات، امکانات و تصمیم‌های ضروری برای ساخت فروشگاه آنلاین با مسیر خرید ساده و پنل مدیریت کاربردی.",
                Keywords: "فروشگاه اینترنتی, طراحی سایت فروشگاهی, پرداخت آنلاین, تجربه خرید, پنل مدیریت",
                CategorySlug: "business",
                PublishedDaysAgo: 5,
                LegacySlug: "building-a-better-digital-business-presence",
                LegacyTitle: "Building a Better Digital Business Presence"),
            new(
                Title: "سرعت سایت چگونه بر تجربه کاربر و فروش اثر می‌گذارد؟",
                Slug: "website-speed-user-experience",
                Excerpt: "رابطه سرعت بارگذاری با اعتماد، ماندگاری کاربر و نرخ تبدیل و چند راهکار عملی برای سریع‌ترشدن وب‌سایت.",
                Content: """
                    چند ثانیه مهم

                    کاربر قبل از دیدن طراحی کامل، سرعت سایت را حس می‌کند. اگر محتوای اصلی دیر ظاهر شود یا دکمه‌ها هنگام بارگذاری جابه‌جا شوند، اعتماد و تمرکز کاربر از بین می‌رود.

                    اثر سرعت بر کسب‌وکار

                    سایت کند نرخ خروج را بالا می‌برد، استفاده در اینترنت موبایل را سخت می‌کند و ممکن است تعداد تکمیل فرم یا خرید را کاهش دهد. موتورهای جست‌وجو نیز تجربه صفحه و شاخص‌های سرعت را در ارزیابی فنی در نظر می‌گیرند.

                    عوامل رایج کندی

                    • تصاویر بزرگ و فشرده‌نشده
                    • جاوااسکریپت بیش از نیاز
                    • فونت‌ها و سرویس‌های خارجی متعدد
                    • درخواست‌های API تکراری یا کند
                    • انیمیشن‌های سنگین در موبایل
                    • نبود کش و فشرده‌سازی مناسب

                    راهکارهای عملی

                    تصاویر را با اندازه واقعی نمایش بهینه کنید، کد صفحات غیرضروری را دیرتر بارگذاری کنید، درخواست‌های تکراری را حذف کنید و عملکرد را روی دستگاه و اینترنت واقعی بسنجید. عدد آزمایشگاه مهم است، اما تجربه کاربر واقعی معیار نهایی است.

                    جمع‌بندی

                    بهینه‌سازی سرعت یک مرحله پس از طراحی نیست؛ باید از معماری، انتخاب تصویر و نحوه دریافت داده تا جزئیات رابط در تمام فرایند توسعه حضور داشته باشد.
                    """,
                CoverImage: "https://images.pexels.com/photos/3761509/pexels-photo-3761509.jpeg?auto=compress&cs=tinysrgb&w=1400",
                ReadTime: 7,
                SeoTitle: "تأثیر سرعت سایت بر تجربه کاربر، سئو و فروش",
                SeoDescription: "دلایل کندی سایت، اثر سرعت بر اعتماد و نرخ تبدیل و راهکارهای عملی برای بهبود تجربه کاربران را بررسی می‌کنیم.",
                Keywords: "سرعت سایت, تجربه کاربری, Core Web Vitals, بهینه‌سازی, نرخ تبدیل",
                CategorySlug: "business",
                PublishedDaysAgo: 6)
        };
    }
}
