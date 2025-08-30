using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DV8.Console.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { error = "internal error" });
                }
            }
        }
    }
}
