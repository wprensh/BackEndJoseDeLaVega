using JoseDeLaVega.Api.Infrastructure;
using JoseDeLaVega.Application.Common;
using JoseDeLaVega.Application.Noticias;
using JoseDeLaVega.Domain.Noticias;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JoseDeLaVega.Api.Endpoints;

/// <summary>
/// CRUD de noticias con Minimal APIs.
/// Cada handler devuelve <c>Results&lt;...&gt;</c> tipados: el compilador verifica las respuestas
/// posibles y OpenAPI las documenta automáticamente, sin atributos [ProducesResponseType].
/// </summary>
internal static class NoticiasEndpoints
{
    public static IEndpointRouteBuilder MapNoticiasEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/noticias")
            .WithTags("Noticias");

        group.MapGet("/", ListarAsync)
            .WithName("ListarNoticias")
            .WithSummary("Lista noticias paginadas con filtros opcionales.");

        group.MapGet("/{id:guid}", ObtenerAsync)
            .WithName("ObtenerNoticia")
            .WithSummary("Obtiene una noticia por su id.");

        // Escritura: solo con la clave de administración.
        group.MapPost("/", CrearAsync)
            .WithName("CrearNoticia")
            .WithSummary("Crea una noticia.")
            .RequiereAdministrador();

        group.MapPut("/{id:guid}", ActualizarAsync)
            .WithName("ActualizarNoticia")
            .WithSummary("Actualiza una noticia existente.")
            .RequiereAdministrador();

        group.MapDelete("/{id:guid}", EliminarAsync)
            .WithName("EliminarNoticia")
            .WithSummary("Elimina una noticia.")
            .RequiereAdministrador();

        return app;
    }

    /// <summary>El público solo ve noticias publicadas; los borradores exigen la clave de administración.</summary>
    private static async Task<Ok<PagedResult<NoticiaResponse>>> ListarAsync(
        HttpContext httpContext,
        INoticiaService service,
        CancellationToken ct,
        string? buscar = null,
        CategoriaNoticia? categoria = null,
        bool soloPublicadas = false,
        int pagina = 1,
        int tamanoPagina = 10)
    {
        var soloVisibles = soloPublicadas || !ClaveAdministracion.EsAdministrador(httpContext);
        var resultado = await service.ListarAsync(new NoticiaQuery(buscar, categoria, soloVisibles, pagina, tamanoPagina), ct);
        return TypedResults.Ok(resultado);
    }

    private static async Task<Results<Ok<NoticiaResponse>, ProblemHttpResult>> ObtenerAsync(
        Guid id, HttpContext httpContext, INoticiaService service, CancellationToken ct)
    {
        var resultado = await service.ObtenerAsync(id, ct);

        // Un borrador no existe para el público.
        if (resultado.IsSuccess && !resultado.Value.Publicada && !ClaveAdministracion.EsAdministrador(httpContext))
        {
            return Error.NoEncontrado("Noticia.NoEncontrada", $"No existe una noticia con id '{id}'.").ToProblem();
        }

        return resultado.Match<Results<Ok<NoticiaResponse>, ProblemHttpResult>>(
            noticia => TypedResults.Ok(noticia),
            error => error.ToProblem());
    }

    private static async Task<Results<CreatedAtRoute<NoticiaResponse>, ProblemHttpResult>> CrearAsync(
        GuardarNoticiaRequest request, INoticiaService service, CancellationToken ct)
    {
        var resultado = await service.CrearAsync(request, ct);
        return resultado.Match<Results<CreatedAtRoute<NoticiaResponse>, ProblemHttpResult>>(
            noticia => TypedResults.CreatedAtRoute(noticia, "ObtenerNoticia", new { id = noticia.Id }),
            error => error.ToProblem());
    }

    private static async Task<Results<Ok<NoticiaResponse>, ProblemHttpResult>> ActualizarAsync(
        Guid id, GuardarNoticiaRequest request, INoticiaService service, CancellationToken ct)
    {
        var resultado = await service.ActualizarAsync(id, request, ct);
        return resultado.Match<Results<Ok<NoticiaResponse>, ProblemHttpResult>>(
            noticia => TypedResults.Ok(noticia),
            error => error.ToProblem());
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> EliminarAsync(
        Guid id, INoticiaService service, CancellationToken ct)
    {
        var resultado = await service.EliminarAsync(id, ct);
        return resultado.Match<Results<NoContent, ProblemHttpResult>>(
            _ => TypedResults.NoContent(),
            error => error.ToProblem());
    }
}
