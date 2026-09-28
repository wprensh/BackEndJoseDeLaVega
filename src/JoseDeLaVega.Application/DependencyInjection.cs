using FluentValidation;
using JoseDeLaVega.Application.Noticias;
using JoseDeLaVega.Application.Pqrs;
using Microsoft.Extensions.DependencyInjection;

namespace JoseDeLaVega.Application;

public static class DependencyInjection
{
    /// <summary>Registra los casos de uso y validadores de la capa de aplicación.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INoticiaService, NoticiaService>();
        services.AddScoped<IPqrsService, PqrsService>();

        // Registra todos los AbstractValidator<T> del ensamblado.
        services.AddValidatorsFromAssemblyContaining<GuardarNoticiaValidator>(ServiceLifetime.Singleton);

        return services;
    }
}
