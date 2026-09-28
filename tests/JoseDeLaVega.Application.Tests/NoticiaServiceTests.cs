using JoseDeLaVega.Application.Abstractions;
using JoseDeLaVega.Application.Common;
using JoseDeLaVega.Application.Noticias;
using JoseDeLaVega.Domain.Noticias;

namespace JoseDeLaVega.Application.Tests;

public sealed class NoticiaServiceTests
{
    private readonly FakeNoticiaRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly NoticiaService _service;

    public NoticiaServiceTests() =>
        _service = new NoticiaService(_repository, _unitOfWork, new GuardarNoticiaValidator());

    private static GuardarNoticiaRequest RequestValido(string titulo = "Semana de la Cultura") =>
        new(titulo, "Exposición artística", "Contenido", CategoriaNoticia.Cultural, new DateOnly(2026, 9, 21), null);

    [Fact]
    public async Task Crear_con_datos_validos_guarda_y_devuelve_la_noticia()
    {
        var resultado = await _service.CrearAsync(RequestValido(), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("Semana de la Cultura", resultado.Value.Titulo);
        Assert.Single(_repository.Noticias);
        Assert.Equal(1, _unitOfWork.Guardados);
    }

    [Fact]
    public async Task Crear_sin_titulo_devuelve_error_de_validacion()
    {
        var resultado = await _service.CrearAsync(RequestValido(titulo: ""), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal(ErrorType.Validacion, resultado.Error.Tipo);
        Assert.Contains(nameof(GuardarNoticiaRequest.Titulo), resultado.Error.ErroresValidacion.Keys);
        Assert.Equal(0, _unitOfWork.Guardados);
    }

    [Fact]
    public async Task Crear_con_titulo_duplicado_devuelve_conflicto()
    {
        await _service.CrearAsync(RequestValido(), CancellationToken.None);

        var resultado = await _service.CrearAsync(RequestValido("SEMANA DE LA CULTURA"), CancellationToken.None);

        Assert.Equal(ErrorType.Conflicto, resultado.Error?.Tipo);
    }

    [Fact]
    public async Task Actualizar_noticia_inexistente_devuelve_no_encontrado()
    {
        var resultado = await _service.ActualizarAsync(Guid.NewGuid(), RequestValido(), CancellationToken.None);

        Assert.Equal(ErrorType.NoEncontrado, resultado.Error?.Tipo);
    }

    [Fact]
    public async Task Eliminar_quita_la_noticia()
    {
        var creada = await _service.CrearAsync(RequestValido(), CancellationToken.None);

        var resultado = await _service.EliminarAsync(creada.Value!.Id, CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Empty(_repository.Noticias);
    }

    [Fact]
    public async Task Listar_normaliza_la_paginacion()
    {
        var pagina = await _service.ListarAsync(new NoticiaQuery(Pagina: -3, TamanoPagina: 5000), CancellationToken.None);

        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(NoticiaQuery.TamanoPaginaMaximo, pagina.TamanoPagina);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int Guardados { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Guardados++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeNoticiaRepository : INoticiaRepository
    {
        public List<Noticia> Noticias { get; } = [];

        public Task<Noticia?> ObtenerPorIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Noticias.FirstOrDefault(n => n.Id == id));

        public Task<PagedResult<Noticia>> ListarAsync(NoticiaQuery query, CancellationToken ct) =>
            Task.FromResult(new PagedResult<Noticia>(
                [.. Noticias.Skip((query.Pagina - 1) * query.TamanoPagina).Take(query.TamanoPagina)],
                query.Pagina, query.TamanoPagina, Noticias.Count));

        public Task<bool> ExisteTituloAsync(string titulo, Guid? excluirId, CancellationToken ct) =>
            Task.FromResult(Noticias.Any(n =>
                string.Equals(n.Titulo, titulo.Trim(), StringComparison.OrdinalIgnoreCase) && n.Id != excluirId));

        public void Agregar(Noticia noticia) => Noticias.Add(noticia);

        public void Eliminar(Noticia noticia) => Noticias.Remove(noticia);
    }
}
