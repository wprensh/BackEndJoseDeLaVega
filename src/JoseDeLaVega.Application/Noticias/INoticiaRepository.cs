using JoseDeLaVega.Application.Common;
using JoseDeLaVega.Domain.Noticias;

namespace JoseDeLaVega.Application.Noticias;

/// <summary>Puerto de persistencia de noticias (lo implementa Infrastructure).</summary>
public interface INoticiaRepository
{
    Task<Noticia?> ObtenerPorIdAsync(Guid id, CancellationToken ct);

    Task<PagedResult<Noticia>> ListarAsync(NoticiaQuery query, CancellationToken ct);

    Task<bool> ExisteTituloAsync(string titulo, Guid? excluirId, CancellationToken ct);

    void Agregar(Noticia noticia);

    void Eliminar(Noticia noticia);
}
