using Application.Common.Exceptions;
using Application.Common.Models;
using System.Net;
using System.Text.Json;
namespace AriansLab.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
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
                    exception,
                    "Handled API exception occurred. Path: {Path}",
                    context.Request.Path);

                await WriteErrorResponseAsync(
                    context,
                    exception.StatusCode,
                    exception.Message,
                    exception.Errors);
            }
            catch (UnauthorizedAccessException exception)
            {
                _logger.LogWarning(
                    exception,
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

                var errors = _environment.IsDevelopment()
                    ? new
                    {
                        exception.Message,
                        exception.StackTrace
                    }
                    : null;

                await WriteErrorResponseAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    errors);
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

            var response = ApiResponse.Fail(message, errors);

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
