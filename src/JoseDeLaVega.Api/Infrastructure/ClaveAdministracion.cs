using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace JoseDeLaVega.Api.Infrastructure;

/// <summary>Configuración de la sección "Administracion" (variable de entorno Administracion__ClaveApi).</summary>
internal sealed class OpcionesAdministracion
{
    public const string Seccion = "Administracion";

    /// <summary>Clave que deben enviar el panel de noticias y la gestión de PQRS. Mínimo 16 caracteres.</summary>
    public string? ClaveApi { get; init; }
}

/// <summary>
/// Protección de los endpoints administrativos con una clave compartida en el encabezado
/// <c>X-Clave-Admin</c>. Es una medida mínima mientras se implementa inicio de sesión por usuario.
/// Si la clave no está configurada, nadie es administrador (falla cerrada).
/// </summary>
internal static class ClaveAdministracion
{
    public const string Encabezado = "X-Clave-Admin";
    private const int LongitudMinima = 16;

    public static bool EsAdministrador(HttpContext httpContext)
    {
        var clave = httpContext.RequestServices.GetRequiredService<IOptions<OpcionesAdministracion>>().Value.ClaveApi;
        if (string.IsNullOrWhiteSpace(clave) || clave.Length < LongitudMinima)
        {
            return false;
        }

        if (!httpContext.Request.Headers.TryGetValue(Encabezado, out var enviada))
        {
            return false;
        }

        // Comparación en tiempo constante: no revela por tiempos cuántos caracteres coinciden.
        var esperada = Encoding.UTF8.GetBytes(clave);
        var recibida = Encoding.UTF8.GetBytes(enviada.ToString());
        return esperada.Length == recibida.Length && CryptographicOperations.FixedTimeEquals(esperada, recibida);
    }

    /// <summary>Responde 401 a quien no envíe la clave de administración correcta.</summary>
    public static TBuilder RequiereAdministrador<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(async (contexto, siguiente) =>
            EsAdministrador(contexto.HttpContext)
                ? await siguiente(contexto)
                : TypedResults.Problem(
                    title: "Se requiere la clave de administración.",
                    statusCode: StatusCodes.Status401Unauthorized));
}
