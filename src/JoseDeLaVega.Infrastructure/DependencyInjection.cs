using JoseDeLaVega.Application.Abstractions;
using JoseDeLaVega.Application.Noticias;
using JoseDeLaVega.Application.Pqrs;
using JoseDeLaVega.Infrastructure.Persistence;
using JoseDeLaVega.Infrastructure.Persistence.Interceptors;
using JoseDeLaVega.Infrastructure.Persistence.Repositories;
using JoseDeLaVega.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JoseDeLaVega.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DefaultConnection";

    /// <summary>Registra EF Core + PostgreSQL, repositorios e interceptores.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión '{ConnectionStringName}'.");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditoriaInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", ApplicationDbContext.Schema);
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                })
                // Tablas y columnas en snake_case, la convención habitual en PostgreSQL.
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>())
                // EF Core 9: siembra de datos integrada en el flujo de migraciones.
                .UseSeeding((ctx, _) => DatosIniciales.Sembrar(ctx))
                .UseAsyncSeeding((ctx, _, ct) => DatosIniciales.SembrarAsync(ctx, ct));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<INoticiaRepository, NoticiaRepository>();
        services.AddScoped<IPqrsRepository, PqrsRepository>();

        return services;
    }
}
