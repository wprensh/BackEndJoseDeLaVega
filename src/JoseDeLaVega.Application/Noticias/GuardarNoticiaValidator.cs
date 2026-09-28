using FluentValidation;
using JoseDeLaVega.Domain.Noticias;

namespace JoseDeLaVega.Application.Noticias;

public sealed class GuardarNoticiaValidator : AbstractValidator<GuardarNoticiaRequest>
{
    public GuardarNoticiaValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(Noticia.TituloMaxLength);

        RuleFor(x => x.Resumen)
            .NotEmpty().WithMessage("El resumen es obligatorio.")
            .MaximumLength(Noticia.ResumenMaxLength);

        RuleFor(x => x.Contenido)
            .MaximumLength(Noticia.ContenidoMaxLength);

        RuleFor(x => x.Categoria)
            .IsInEnum().WithMessage("La categoría no es válida.");

        RuleFor(x => x.FechaPublicacion)
            .NotEmpty().WithMessage("La fecha de publicación es obligatoria.");

        RuleFor(x => x.ImagenUrl)
            .MaximumLength(Noticia.ImagenUrlMaxLength)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            .When(x => !string.IsNullOrWhiteSpace(x.ImagenUrl))
            .WithMessage("La URL de la imagen debe ser una dirección http(s) válida.");
    }
}
