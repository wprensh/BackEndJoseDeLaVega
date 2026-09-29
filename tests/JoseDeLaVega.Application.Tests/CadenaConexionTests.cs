using JoseDeLaVega.Infrastructure.Persistence;
using Npgsql;

namespace JoseDeLaVega.Application.Tests;

public sealed class CadenaConexionTests
{
    [Fact]
    public void Convierte_la_url_de_Neon_al_formato_de_Npgsql()
    {
        var cadena = CadenaConexion.Normalizar(
            "postgresql://colegio_owner:cl%40ve@ep-ejemplo-123.us-east-1.aws.neon.tech/jose_de_la_vega?sslmode=require&channel_binding=require");

        var b = new NpgsqlConnectionStringBuilder(cadena);
        Assert.Equal("ep-ejemplo-123.us-east-1.aws.neon.tech", b.Host);
        Assert.Equal(5432, b.Port);
        Assert.Equal("jose_de_la_vega", b.Database);
        Assert.Equal("colegio_owner", b.Username);
        Assert.Equal("cl@ve", b.Password);
        Assert.Equal(SslMode.Require, b.SslMode);
    }

    [Fact]
    public void Respeta_el_puerto_de_la_url()
    {
        var b = new NpgsqlConnectionStringBuilder(CadenaConexion.Normalizar("postgres://u:p@servidor:6543/base"));
        Assert.Equal(6543, b.Port);
    }

    [Fact]
    public void Deja_igual_una_cadena_en_formato_Npgsql()
    {
        const string cadena = "Host=localhost;Port=5432;Database=jose_de_la_vega;Username=postgres;Password=postgres";
        Assert.Equal(cadena, CadenaConexion.Normalizar(cadena));
    }
}
