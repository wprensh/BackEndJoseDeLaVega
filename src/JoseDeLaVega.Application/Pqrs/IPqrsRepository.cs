using JoseDeLaVega.Application.Common;
using JoseDeLaVega.Domain.Pqrs;

namespace JoseDeLaVega.Application.Pqrs;

public interface IPqrsRepository
{
    Task<SolicitudPqrs?> ObtenerPorIdAsync(Guid id, CancellationToken ct);

    Task<PagedResult<SolicitudPqrs>> ListarAsync(PqrsQuery query, CancellationToken ct);

    void Agregar(SolicitudPqrs solicitud);
}
