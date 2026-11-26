using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;
using TaskManagementAPI.Helper;

namespace TaskManagementAPI.Middleware
{
    public class ExceptionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;
        public ExceptionsMiddleware(RequestDelegate next, IHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            ApplySecurity(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = _environment.IsDevelopment() ?
                     new ApiExceptions((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace)
                    : new ApiExceptions((int)HttpStatusCode.InternalServerError, ex.Message,ex.StackTrace);

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
        private void ApplySecurity(HttpContext context)
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";

        }
    }
}
