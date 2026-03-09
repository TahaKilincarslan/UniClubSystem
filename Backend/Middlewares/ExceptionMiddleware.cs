using System.Net;
using UniversityClubSystem.DTOs;

namespace UniversityClubSystem.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new ErrorResponseDto
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message,
                    Details = _env.IsDevelopment() ? ex.StackTrace?.ToString() : "An internal server error occurred."
                };

                await context.Response.WriteAsync(response.ToString());
            }
        }
    }
}
