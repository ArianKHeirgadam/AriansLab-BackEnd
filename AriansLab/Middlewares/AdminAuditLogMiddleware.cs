using Application.DTOs.Logs;
using Application.Interfaces;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AriansLab.Api.Middlewares;

public class AdminAuditLogMiddleware
{
    private const int MaxBodyLength = 20_000;
    private static readonly string[] SensitiveKeyFragments =
    [
        "password", "token", "secret", "apikey", "connectionstring"
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<AdminAuditLogMiddleware> _logger;

    public AdminAuditLogMiddleware(
        RequestDelegate next,
        ILogger<AdminAuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuditLogWriteService auditLogWriteService)
    {
        var shouldAudit = ShouldAudit(context);

        string? requestBody = null;

        if (shouldAudit && CanReadBody(context))
        {
            requestBody = await ReadRequestBodyAsync(context);
        }

        await _next(context);

        if (!shouldAudit)
        {
            return;
        }

        if (context.Response.StatusCode < StatusCodes.Status200OK ||
            context.Response.StatusCode >= StatusCodes.Status300MultipleChoices)
        {
            return;
        }

        try
        {
            var auditRequest = new CreateAuditLogRequestDto
            {
                UserId = GetUserId(context),
                Action = ResolveAction(context.Request.Method),
                EntityName = ResolveEntityName(context.Request.Path),
                EntityId = ResolveEntityId(context.Request.Path),
                OldValues = null,
                NewValues = requestBody,
                IpAddress = GetClientIpAddress(context),
                UserAgent = GetUserAgent(context)
            };

            await auditLogWriteService.CreateAsync(
                auditRequest,
                context.RequestAborted
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to write admin audit log for path {Path}.",
                context.Request.Path.Value
            );
        }
    }

    private static bool ShouldAudit(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (!path.StartsWith(
                "/api/admin/",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HttpMethods.IsPost(context.Request.Method)
               || HttpMethods.IsPut(context.Request.Method)
               || HttpMethods.IsPatch(context.Request.Method)
               || HttpMethods.IsDelete(context.Request.Method);
    }

    private static bool CanReadBody(HttpContext context)
    {
        if (HttpMethods.IsDelete(context.Request.Method))
        {
            return false;
        }

        if (!context.Request.ContentLength.HasValue ||
            context.Request.ContentLength.Value <= 0)
        {
            return false;
        }

        var contentType = context.Request.ContentType;

        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.Contains(
                "multipart/form-data",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        context.Request.Body.Position = 0;

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true
        );

        var buffer = new char[MaxBodyLength + 1];
        var read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);

        context.Request.Body.Position = 0;

        if (read == 0)
        {
            return null;
        }

        if (read > MaxBodyLength)
        {
            return JsonSerializer.Serialize(new { truncated = true });
        }

        var body = new string(buffer, 0, read);
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var node = JsonNode.Parse(body);
            Redact(node);
            return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void Redact(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                var normalizedKey = property.Key.Replace("_", string.Empty, StringComparison.Ordinal)
                    .Replace("-", string.Empty, StringComparison.Ordinal)
                    .ToLowerInvariant();

                if (SensitiveKeyFragments.Any(fragment => normalizedKey.EndsWith(fragment, StringComparison.Ordinal)))
                {
                    jsonObject[property.Key] = "[REDACTED]";
                }
                else
                {
                    Redact(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                Redact(item);
            }
        }
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var userIdValue =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue("userId");

        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }

    private static string ResolveAction(string method)
    {
        if (HttpMethods.IsPost(method))
        {
            return "Create";
        }

        if (HttpMethods.IsPut(method))
        {
            return "Update";
        }

        if (HttpMethods.IsPatch(method))
        {
            return "Patch";
        }

        if (HttpMethods.IsDelete(method))
        {
            return "Delete";
        }

        return method.ToUpperInvariant();
    }

    private static string ResolveEntityName(PathString path)
    {
        var segments = GetPathSegments(path);

        var adminIndex = Array.FindIndex(
            segments,
            x => x.Equals("admin", StringComparison.OrdinalIgnoreCase)
        );

        if (adminIndex < 0 || adminIndex + 1 >= segments.Length)
        {
            return "Unknown";
        }

        return ToPascalCase(segments[adminIndex + 1]);
    }

    private static string ResolveEntityId(PathString path)
    {
        var segments = GetPathSegments(path);

        foreach (var segment in segments)
        {
            if (Guid.TryParse(segment, out _))
            {
                return segment;
            }
        }

        return string.Empty;
    }

    private static string[] GetPathSegments(PathString path)
    {
        return path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            ?? Array.Empty<string>();
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        var parts = value.Split(
            new[] { '-', '_' },
            StringSplitOptions.RemoveEmptyEntries
        );

        if (parts.Length == 0)
        {
            return "Unknown";
        }

        return string.Concat(
            parts.Select(part =>
                char.ToUpperInvariant(part[0]) +
                part[1..].ToLowerInvariant())
        );
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetUserAgent(HttpContext context)
    {
        return context.Request.Headers.UserAgent.FirstOrDefault();
    }
}
