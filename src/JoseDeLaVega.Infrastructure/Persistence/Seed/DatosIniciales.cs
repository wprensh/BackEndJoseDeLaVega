using JoseDeLaVega.Domain.Noticias;
using Microsoft.EntityFrameworkCore;

namespace JoseDeLaVega.Infrastructure.Persistence.Seed;

/// <summary>
/// Datos iniciales tomados de las plantillas institucionales.
/// Se registran con <c>UseSeeding</c> / <c>UseAsyncSeeding</c>, la API de siembra de EF Core 9,
/// que se ejecuta al aplicar migraciones (<c>Database.MigrateAsync</c> o <c>dotnet ef database update</c>).
/// EF recomienda implementar ambas: la herramienta de línea de comandos usa la versión síncrona.
/// </summary>
internal static class DatosIniciales
{
    public static void Sembrar(DbContext context)
    {
        var noticias = context.Set<Noticia>();
        if (noticias.Any())
        {
            return;
        }

        noticias.AddRange(CrearNoticias());
        context.SaveChanges();
    }

    public static async Task SembrarAsync(DbContext context, CancellationToken ct)
    {
        var noticias = context.Set<Noticia>();
        if (await noticias.AnyAsync(ct))
        {
            return;
        }

        noticias.AddRange(CrearNoticias());
        await context.SaveChangesAsync(ct);
    }

    private static Noticia[] CrearNoticias() =>
    [
        Noticia.Crear(
            "Semana de la Cultura",
            "Exposición artística y folclórica en la Sede Principal.",
            "Durante la Semana de la Cultura los estudiantes presentan muestras artísticas, danzas y música tradicional del Caribe colombiano.",
            CategoriaNoticia.Cultural,
            new DateOnly(2026, 9, 21)),
        Noticia.Crear(
            "Excelencia Pruebas Saber",
            "Reconocimiento a estudiantes destacados de grado 11°.",
            "La institución reconoce a los estudiantes de grado 11° con los mejores resultados en las Pruebas Saber 11.",
            CategoriaNoticia.Academica,
            new DateOnly(2026, 9, 15)),
        Noticia.Crear(
            "Entrega de Boletines I Periodo",
            "Calendario académico: entrega de boletines del primer periodo.",
            "Los acudientes deben asistir a la entrega de boletines del I periodo en la sede correspondiente.",
            CategoriaNoticia.Evento,
            new DateOnly(2026, 5, 25)),
    ];
}
