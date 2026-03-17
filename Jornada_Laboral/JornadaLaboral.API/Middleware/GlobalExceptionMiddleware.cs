// Red de seguridad: captura cualquier error inesperado antes de que llegue al usuario
using System.Net;
using System.Text.Json;

namespace JornadaLaboral.API.Middleware;

public class GlobalExceptionMiddleware
{
    // _next representa el siguiente paso en la cadena de procesamiento
    private readonly RequestDelegate _next;

    // _logger escribe los errores en la consola para que el desarrollador los vea
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Se ejecuta en cada petición HTTP que entra al sistema
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Deja pasar la petición normalmente
            await _next(context);
        }
        catch (Exception ex)
        {
            // Si algo falla en cualquier parte, lo atrapa aquí
            _logger.LogError(ex, "Error no controlado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    // Construye y envía una respuesta de error limpia al frontend
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Indica que la respuesta es JSON
        context.Response.ContentType = "application/json";

        // Código HTTP 500: error interno del servidor
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Mensaje de error genérico que verá el usuario
        var response = new
        {
            Success   = false,
            ErrorCode = "E00",
            Message   = "Ocurrió un error interno en el servidor. Por favor intente de nuevo.",
            Detail    = ex.Message  // Detalle técnico
        };

        // Serializa el objeto a texto JSON con formato camelCase
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}