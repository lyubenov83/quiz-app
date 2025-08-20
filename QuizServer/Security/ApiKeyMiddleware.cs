using System.Threading.Tasks;

namespace QuizServer.Security
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _validKeys;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;

            // Read multiple API keys from appsettings: "ApiKeys": ["KEY1","KEY2"]
            var keys = config.GetSection("ApiKeys").Get<string[]>() ?? Array.Empty<string>();
            _validKeys = new HashSet<string>(keys.Where(k => !string.IsNullOrWhiteSpace(k)),
                                             StringComparer.Ordinal);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Require API key ONLY for these endpoints:
            var requiresKey =
                path.StartsWith("/api/questions/normal") ||
                path.StartsWith("/api/questions/survival");

            if (!requiresKey)
            {
                await _next(context);
                return;
            }

            // If no keys configured, block by default (safer)
            if (_validKeys.Count == 0)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: No API keys configured.");
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-API-KEY", out var provided) ||
                string.IsNullOrWhiteSpace(provided) ||
                !_validKeys.Contains(provided!))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Missing or invalid API key.");
                return;
            }

            await _next(context);
        }
    }
}
