using System.Net;
using System.Text.Json;

namespace APIAgroConnect.Common.Middleware
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
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
            catch (InvalidOperationException ex)
            {
                // Errores de negocio (validaciones, datos duplicados, etc.)
                _logger.LogWarning(ex, "Error de negocio: {Message}", ex.Message);
                await WriteResponse(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado: {Message}", ex.Message);
                await WriteResponse(context, HttpStatusCode.Unauthorized, "No autorizado.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argumento inválido: {Message}", ex.Message);
                await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                // Error inesperado: loguear completo pero NO exponer al usuario
                _logger.LogError(ex, "Error no manejado en {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                var message = _env.IsDevelopment()
                    ? ex.Message     // En desarrollo sí mostramos el mensaje
                    : "Ocurrió un error interno. Intentá de nuevo más tarde.";

                await WriteResponse(context, HttpStatusCode.InternalServerError, message);
            }
        }

        private static async Task WriteResponse(HttpContext context, HttpStatusCode code, string message)
        {
            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(body);
        }
    }
}
