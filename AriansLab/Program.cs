using Application;
using Application.Common.Models;
using Application.Security;
using Application.Settings;
using AriansLab.Api.Middlewares;
using AriansLab.Api.Security;
using AriansLab.Api.Seed;
using Asp.Versioning;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence;
using Persistence.Context;
using Serilog;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "Logs/arianslab-api-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2 * 1024 * 1024;
});

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException("JWT Issuer and Audience are required.");
}

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 64)
{
    throw new InvalidOperationException("JWT SecretKey must be a random value of at least 64 characters.");
}

if (jwtSettings.AccessTokenExpirationMinutes is < 5 or > 30)
{
    throw new InvalidOperationException("JWT access-token lifetime must be between 5 and 30 minutes.");
}

if (jwtSettings.RefreshTokenExpirationDays is < 1 or > 30)
{
    throw new InvalidOperationException("JWT refresh-token lifetime must be between 1 and 30 days.");
}

var authCookieSettings = builder.Configuration
    .GetSection(AuthCookieSettings.SectionName)
    .Get<AuthCookieSettings>() ?? new AuthCookieSettings();

if (!authCookieSettings.Secure)
{
    throw new InvalidOperationException("Authentication cookies must always be Secure.");
}

var cookieSameSite = ParseSameSite(authCookieSettings.SameSite);
if (cookieSameSite == SameSiteMode.None && !authCookieSettings.Secure)
{
    throw new InvalidOperationException("SameSite=None cookies must be Secure.");
}

builder.Services.AddSingleton(jwtSettings);
builder.Services.Configure<AuthCookieSettings>(
    builder.Configuration.GetSection(AuthCookieSettings.SectionName));

builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .ToDictionary(
                    item => item.Key,
                    item => item.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            return new BadRequestObjectResult(ApiResponse.Fail("Validation failed.", errors));
        };
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = AuthCookieSettings.AntiforgeryCookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = cookieSameSite;
    options.Cookie.Path = "/";
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token) &&
                    context.Request.Cookies.TryGetValue(AuthCookieSettings.AccessCookieName, out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var roleValue = context.Principal?.FindFirstValue(ClaimTypes.Role);
                var stampValue = context.Principal?.FindFirstValue(UserSecurityStamp.ClaimType);

                if (!Guid.TryParse(userIdValue, out var userId) ||
                    string.IsNullOrWhiteSpace(roleValue) ||
                    string.IsNullOrWhiteSpace(stampValue))
                {
                    context.Fail("The authenticated user claims are invalid.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var user = await dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item => item.Id == userId, context.HttpContext.RequestAborted);

                if (user is null ||
                    !user.IsActive ||
                    !string.Equals(user.Role.ToString(), roleValue, StringComparison.Ordinal) ||
                    !string.Equals(UserSecurityStamp.Create(user), stampValue, StringComparison.Ordinal))
                {
                    context.Fail("The authenticated user is inactive or their security data changed.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray() ?? Array.Empty<string>();

if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins = ["http://localhost:3000", "https://localhost:3000"];
}

if (allowedOrigins.Length == 0 || allowedOrigins.Any(origin => !IsValidOrigin(origin)))
{
    throw new InvalidOperationException("Cors:AllowedOrigins must contain exact valid origins.");
}

if (builder.Environment.IsProduction() &&
    allowedOrigins.Any(origin => !origin.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException("Production CORS origins must use HTTPS.");
}

if (builder.Environment.IsProduction())
{
    var allowedHosts = builder.Configuration["AllowedHosts"];
    if (string.IsNullOrWhiteSpace(allowedHosts) || allowedHosts.Trim() == "*")
    {
        throw new InvalidOperationException("AllowedHosts must be restricted in production.");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.RequireHeaderSymmetry = true;

    foreach (var value in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
    {
        if (IPAddress.TryParse(value, out var address))
        {
            options.KnownProxies.Add(address);
        }
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => CreateFixedWindow(context, 10, TimeSpan.FromMinutes(1)));
    options.AddPolicy("csrf", context => CreateFixedWindow(context, 60, TimeSpan.FromMinutes(1)));
    options.AddPolicy("public-write", context => CreateFixedWindow(context, 30, TimeSpan.FromMinutes(1)));
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (!context.HttpContext.Response.HasStarted)
        {
            await context.HttpContext.Response.WriteAsJsonAsync(
                ApiResponse.Fail("Too many requests. Please try again later."),
                cancellationToken);
        }
    };
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AriansLab API",
        Version = "v1",
        Description = "Browser authentication uses Secure HttpOnly cookies. Obtain a CSRF token before unsafe requests."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Optional Bearer token support for trusted non-browser clients.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await AdminSeeder.SeedAsync(app.Services);
    await BlogSeeder.SeedAsync(app.Services);
    await PortfolioSeeder.SeedAsync(app.Services);
    await PricingSeeder.SeedAsync(app.Services);
    await ServicesSeeder.SeedAsync(app.Services);
}

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
        context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
        context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
        context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        if (!app.Environment.IsDevelopment() || !context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Response.Headers.TryAdd(
                "Content-Security-Policy",
                "default-src 'none'; frame-ancestors 'none'; form-action 'none'; base-uri 'none'");
        }

        if (context.Request.Path.StartsWithSegments("/api/Auth"))
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.Pragma = "no-cache";
        }

        return Task.CompletedTask;
    });

    await next();
});

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("FrontendClient");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AdminAuditLogMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.MapGet("/", () => Results.Ok(new { service = "AriansLab API", status = "ok" }));
}

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

static RateLimitPartition<string> CreateFixedWindow(HttpContext context, int permitLimit, TimeSpan window)
{
    var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    return RateLimitPartition.GetFixedWindowLimiter(
        key,
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0,
            AutoReplenishment = true
        });
}

static bool IsValidOrigin(string? origin)
{
    return Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
           (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
           string.IsNullOrEmpty(uri.PathAndQuery.Trim('/')) &&
           string.IsNullOrEmpty(uri.Fragment);
}

static SameSiteMode ParseSameSite(string value) => value.Trim().ToLowerInvariant() switch
{
    "strict" => SameSiteMode.Strict,
    "none" => SameSiteMode.None,
    _ => SameSiteMode.Lax
};

public partial class Program;
