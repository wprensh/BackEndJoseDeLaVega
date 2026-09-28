using Microsoft.AspNetCore.Diagnostics;

namespace JoseDeLaVega.Api.Infrastructure;

/// <summary>
/// Manejo centralizado de excepciones no controladas (<see cref="IExceptionHandler"/>).
/// Registra el error y responde ProblemDetails sin exponer detalles internos al cliente.
/// </summary>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogExcepcion(logger, httpContext.Request.Method, httpContext.Request.Path, exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = "Ocurrió un error inesperado.",
                Status = StatusCodes.Status500InternalServerError,
            },
        });
    }

    // Logging de alto rendimiento generado en compilación (LoggerMessage source generator).
    [LoggerMessage(Level = LogLevel.Error, Message = "Error no controlado en {Metodo} {Ruta}")]
    private static partial void LogExcepcion(ILogger logger, string metodo, string ruta, Exception exception);
}
