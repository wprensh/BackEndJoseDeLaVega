using JoseDeLaVega.Domain.Noticias;

namespace JoseDeLaVega.Application.Noticias;

/// <summary>Datos que devuelve la API para una noticia.</summary>
public sealed record NoticiaResponse(
    Guid Id,
    string Titulo,
    string Resumen,
    string Contenido,
    CategoriaNoticia Categoria,
    string? ImagenUrl,
    DateOnly FechaPublicacion,
    bool Publicada,
    DateTimeOffset CreadoEn,
    DateTimeOffset? ModificadoEn)
{
    public static NoticiaResponse Desde(Noticia n) => new(
        n.Id, n.Titulo, n.Resumen, n.Contenido, n.Categoria, n.ImagenUrl,
        n.FechaPublicacion, n.Publicada, n.CreadoEn, n.ModificadoEn);
}

/// <summary>Cuerpo de las solicitudes de creación y edición (POST / PUT).</summary>
public sealed record GuardarNoticiaRequest(
    string Titulo,
    string Resumen,
    string? Contenido,
    CategoriaNoticia Categoria,
    DateOnly FechaPublicacion,
    string? ImagenUrl,
    bool Publicada = true);

/// <summary>Filtros y paginación del listado de noticias.</summary>
public sealed record NoticiaQuery(
    string? Buscar = null,
    CategoriaNoticia? Categoria = null,
    bool SoloPublicadas = false,
    int Pagina = 1,
    int TamanoPagina = 10)
{
    public const int TamanoPaginaMaximo = 100;
}
