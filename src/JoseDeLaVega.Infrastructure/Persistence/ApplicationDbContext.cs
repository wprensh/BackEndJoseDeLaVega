using JoseDeLaVega.Application.Abstractions;
using JoseDeLaVega.Domain.Noticias;
using JoseDeLaVega.Domain.Pqrs;
using Microsoft.EntityFrameworkCore;

namespace JoseDeLaVega.Infrastructure.Persistence;

/// <summary>
/// DbContext principal (Code-First). Implementa <see cref="IUnitOfWork"/> para que la capa
/// de aplicación confirme transacciones sin depender de EF Core.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public const string Schema = "colegio";

    public DbSet<Noticia> Noticias => Set<Noticia>();

    public DbSet<SolicitudPqrs> SolicitudesPqrs => Set<SolicitudPqrs>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        // Aplica todas las clases IEntityTypeConfiguration<T> del ensamblado (Fluent API).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Los enums se guardan como texto: la base de datos queda legible y estable ante reordenamientos.
        configurationBuilder.Properties<CategoriaNoticia>().HaveConversion<string>().HaveMaxLength(30);
        configurationBuilder.Properties<TipoPqrs>().HaveConversion<string>().HaveMaxLength(20);
        configurationBuilder.Properties<EstadoPqrs>().HaveConversion<string>().HaveMaxLength(20);
    }
}
