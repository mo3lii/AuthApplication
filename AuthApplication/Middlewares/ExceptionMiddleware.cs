namespace AuthApplication.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            if (exception is UnauthorizedAccessException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Unauthorized access."
                }));
            }
            else if (exception is KeyNotFoundException)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Resource not found."
                }));
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An internal server error occurred.",
                Detailed = exception.Message
            }));
        }
    }
}
