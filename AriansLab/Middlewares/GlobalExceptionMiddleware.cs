using Application.Common.Exceptions;
using Application.Common.Models;
using System.Text.Json;
namespace AriansLab.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException exception)
            {
                _logger.LogWarning(
                    "Handled API exception occurred. Path: {Path}; StatusCode: {StatusCode}",
                    context.Request.Path,
                    exception.StatusCode);

                await WriteErrorResponseAsync(
                    context,
                    exception.StatusCode,
                    exception.Message,
                    exception.Errors);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning(
                    "Unauthorized access exception occurred. Path: {Path}",
                    context.Request.Path);

                await WriteErrorResponseAsync(
                    context,
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized access.",
                    null);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception occurred. Path: {Path}",
                    context.Request.Path);

                await WriteErrorResponseAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    new { traceId = context.TraceIdentifier });
            }
        }

        private static async Task WriteErrorResponseAsync(
            HttpContext context,
            int statusCode,
            string message,
            object? errors)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            context.Response.Headers.CacheControl = "no-store";

            var response = ApiResponse.Fail(message, errors);

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json, context.RequestAborted);
        }
    }
}
