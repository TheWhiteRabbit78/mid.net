using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AbySalto.Mid.WebApi.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions and returns a consistent <see cref="ProblemDetails"/> response.
    /// Stack traces are only included when the environment is Development.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment environment)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing {method} {path}", context.Request.Method, context.Request.Path);
                await WriteProblemAsync(context, ex);
            }
        }

        private async Task WriteProblemAsync(HttpContext context, Exception exception)
        {
            ProblemDetails problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Instance = context.Request.Path,
            };

            if (_environment.IsDevelopment())
            {
                problem.Detail = exception.ToString();
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
