// File: Security/ApiKeyExtensions.cs
namespace QuizServer.Security
{
    public static class ApiKeyExtensions
    {
        public static IApplicationBuilder UseApiKey(this IApplicationBuilder app)
            => app.UseMiddleware<ApiKeyMiddleware>();
    }
}
