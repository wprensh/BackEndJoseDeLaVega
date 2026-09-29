# Backend · I.E. José de la Vega

API REST del sitio web de la **Institución Educativa José de la Vega** (Cartagena de Indias).
Construida con **.NET 9 Minimal APIs**, **Arquitectura Limpia**, **Entity Framework Core 9** (Code-First) y **PostgreSQL**.

Frontend: [FrontEndJoseDeLaVega](https://github.com/wprensh/FrontEndJoseDeLaVega)

## Arquitectura

```
BackEndJoseDeLaVega/
├── src/
│   ├── JoseDeLaVega.Domain/            # Entidades y reglas de negocio (sin dependencias)
│   │   ├── Common/Entity.cs            #   Base: Id Guid v7 + fechas de auditoría
│   │   ├── Noticias/                   #   Noticia, CategoriaNoticia
│   │   └── Pqrs/                       #   SolicitudPqrs, TipoPqrs, EstadoPqrs
│   ├── JoseDeLaVega.Application/       # Casos de uso (servicios), DTOs, validaciones, puertos
│   │   ├── Abstractions/IUnitOfWork.cs
│   │   ├── Common/                     #   Result<T>, Error, PagedResult<T>
│   │   ├── Noticias/                   #   INoticiaService, NoticiaService, INoticiaRepository, validador
│   │   └── Pqrs/
│   ├── JoseDeLaVega.Infrastructure/    # EF Core 9 + PostgreSQL (adaptadores)
│   │   └── Persistence/
│   │       ├── ApplicationDbContext.cs
│   │       ├── Configurations/         #   Fluent API por entidad
│   │       ├── Interceptors/           #   Auditoría automática (SaveChangesInterceptor)
│   │       ├── Repositories/
│   │       ├── Seed/                   #   Datos iniciales (UseAsyncSeeding de EF Core 9)
│   │       └── Migrations/
│   └── JoseDeLaVega.Api/               # Minimal APIs, DI, CORS, ProblemDetails, OpenAPI
│       ├── Endpoints/                  #   NoticiasEndpoints, PqrsEndpoints
│       └── Program.cs
├── tests/JoseDeLaVega.Application.Tests/   # Pruebas unitarias (xUnit)
├── db/InicialColegio.sql               # Script SQL idempotente generado desde las migraciones
└── docker-compose.yml                  # PostgreSQL 17 local
```

Las dependencias apuntan hacia adentro: `Api → Infrastructure → Application → Domain`.
La capa de aplicación define interfaces (`INoticiaRepository`, `IUnitOfWork`) y la infraestructura las implementa.

## Requisitos

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL 15+ (o Docker)

## Ejecutar en local

```bash
# 1. Base de datos
docker compose up -d

# 2. Herramientas locales (dotnet-ef)
dotnet tool restore

# 3. API (en Development aplica las migraciones y los datos iniciales al arrancar)
dotnet run --project src/JoseDeLaVega.Api
```

- Documentación interactiva (Scalar): http://localhost:5075/scalar/v1
- OpenAPI: http://localhost:5075/openapi/v1.json
- Salud: http://localhost:5075/health

## Configuración

La cadena de conexión está en `src/JoseDeLaVega.Api/appsettings.json` (`ConnectionStrings:DefaultConnection`).
En producción **no** la guardes en el repositorio: usa una variable de entorno o un gestor de secretos.

```bash
# Variable de entorno (el doble guion bajo equivale a ":")
ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=..."
```

Los orígenes permitidos por CORS se configuran en `Cors:OrigenesPermitidos`.

## Migraciones (Code-First)

```bash
# Crear una migración después de cambiar el modelo
dotnet ef migrations add NombreDelCambio -p src/JoseDeLaVega.Infrastructure -s src/JoseDeLaVega.Api -o Persistence/Migrations

# Aplicarla
dotnet ef database update -p src/JoseDeLaVega.Infrastructure -s src/JoseDeLaVega.Api

# Script SQL idempotente para producción
dotnet ef migrations script -p src/JoseDeLaVega.Infrastructure -s src/JoseDeLaVega.Api --idempotent -o db/migracion.sql
```

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/noticias?buscar=&categoria=&soloPublicadas=&pagina=&tamanoPagina=` | Lista paginada |
| GET | `/api/noticias/{id}` | Detalle |
| POST | `/api/noticias` | Crear |
| PUT | `/api/noticias/{id}` | Actualizar |
| DELETE | `/api/noticias/{id}` | Eliminar |
| POST | `/api/pqrs` | Radicar PQRS (formulario de contacto, limitado a 5/min por IP) |
| GET | `/api/pqrs?estado=` | Listar PQRS |
| PUT | `/api/pqrs/{id}/respuesta` | Responder PQRS |
| GET | `/health` | Estado de la API y de PostgreSQL |

Los errores siguen el estándar **ProblemDetails (RFC 9457)**; los de validación incluyen el diccionario `errors` por campo.
Los enums viajan como texto (`"Cultural"`, `"Academica"`, ...).

## Características de C# 13 / .NET 9 usadas

- **Constructores primarios** en servicios, repositorios, DbContext e interceptores.
- **Expresiones de colección** (`[]`, `[.. items]`).
- **Colecciones `params` (C# 13)** en `Error.Validacion(params IEnumerable<...>)`.
- **`Guid.CreateVersion7()` (.NET 9)**: ids ordenables por tiempo, mejores para índices B-tree.
- **OpenAPI nativo** (`AddOpenApi` / `MapOpenApi`) sin Swashbuckle, con **Scalar** como UI.
- **EF Core 9 `UseSeeding` / `UseAsyncSeeding`** para los datos iniciales.
- **`TypedResults` + `Results<...>`**: respuestas tipadas y documentadas automáticamente.
- `TimeProvider`, `IExceptionHandler`, `LoggerMessage` (logging generado en compilación), rate limiting nativo.

> Sobre los **interceptores de C#** (característica del compilador): no se usan directamente porque son una
> herramienta para generadores de código; ASP.NET Core ya los aprovecha internamente con el Request Delegate
> Generator en escenarios AOT. En este proyecto sí se usa un **interceptor de EF Core** para la auditoría.

## Pruebas

```bash
dotnet test
```

## Clave de administración

Crear, editar y eliminar noticias, y listar o responder PQRS, exige el encabezado `X-Clave-Admin` con el valor
de `Administracion:ClaveApi` (mínimo 16 caracteres). Sin la clave, la API solo muestra noticias publicadas.
Si la clave no está configurada, nadie tiene acceso administrativo.

En desarrollo la clave se guarda con *user-secrets* (fuera del repositorio):

```bash
dotnet user-secrets set "Administracion:ClaveApi" "una-clave-larga-y-privada" --project src/JoseDeLaVega.Api
dotnet user-secrets list --project src/JoseDeLaVega.Api
```

## Despliegue en Render + Neon

El repositorio incluye `Dockerfile` y `render.yaml` (Blueprint).

1. **Base de datos:** crear una cuenta en [neon.com](https://neon.com), un proyecto en la región *AWS US East
   (N. Virginia)* y copiar la *connection string* (`postgresql://...`).
2. **API:** en [render.com](https://render.com), *New → Blueprint*, conectar este repositorio de GitHub.
3. Cuando Render lo pida, pegar la cadena de Neon en `ConnectionStrings__DefaultConnection`. Se acepta tal cual,
   en formato URL.
4. Al terminar el despliegue, la API queda en `https://josedelavega-api.onrender.com`. Al arrancar aplica las
   migraciones y carga los datos iniciales.
5. La clave de administración la genera Render: *Environment → Administracion__ClaveApi*. Esa clave se escribe en
   el panel `/admin/noticias` del sitio.
6. Si Render asigna otra dirección, actualizar `apiBaseUrl` en `src/environments/environment.ts` del frontend.

Variables de entorno (definidas en `render.yaml`):

| Variable | Uso |
|---|---|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión de PostgreSQL (Neon) |
| `Administracion__ClaveApi` | Clave del panel administrativo |
| `Cors__OrigenesPermitidos__0` | Sitio que puede llamar a la API (`https://wprensh.github.io`) |
| `Database__AplicarMigraciones` | `true`: aplica migraciones al arrancar |
| `DetrasDeProxy` | `true`: confía en `X-Forwarded-For` del proxy de Render para la IP real |

## Pendiente

- Inicio de sesión por usuario (JWT o Microsoft Entra ID, aprovechando el Microsoft 365 del colegio) en lugar de
  una clave compartida.
- Actualizar a .NET 10 (LTS): el soporte de .NET 9 termina el 10 de noviembre de 2026.
