using System.Text.Json;
namespace CosmaAPI.middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger
        )
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
            _logger.LogError(ex, "Una exception no controlada ocurrió.");

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var (statusCode, message) = exception switch
        {
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Operación no válida."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Acceso no autorizado."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado."),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.")
        };
        context.Response.StatusCode = statusCode;

        var response = new
        {
            message,
            statusCode
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}