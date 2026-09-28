using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using JoseDeLaVega.Api.Endpoints;
using JoseDeLaVega.Api.Infrastructure;
using JoseDeLaVega.Application;
using JoseDeLaVega.Infrastructure;
using JoseDeLaVega.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Capas de la arquitectura limpia ----------
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// ---------- Serialización JSON ----------
// Los enums viajan como texto ("Cultural", "Academica"...), más legible para el frontend.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---------- Errores: ProblemDetails (RFC 9457) + manejador global ----------
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---------- OpenAPI nativo de .NET 9 (sin Swashbuckle) ----------
builder.Services.AddOpenApi();

// ---------- CORS para el frontend Angular ----------
const string FrontendCorsPolicy = "frontend";
var origenesPermitidos = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(origenesPermitidos)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ---------- Rate limiting del formulario público PQRS ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(PqrsEndpoints.RateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
            }));
});

// ---------- Health checks (incluye la conexión a PostgreSQL) ----------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("postgresql");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                       // /openapi/v1.json
    app.MapScalarApiReference(options =>    // /scalar/v1 : documentación interactiva
        options.WithTitle("API I.E. José de la Vega"));

    // En desarrollo aplica las migraciones pendientes al arrancar (y ejecuta la siembra de EF Core 9).
    // En producción se recomienda ejecutar las migraciones en el pipeline de despliegue.
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapNoticiasEndpoints();
app.MapPqrsEndpoints();

await app.RunAsync();

/// <summary>Expuesta para pruebas de integración con WebApplicationFactory.</summary>
public partial class Program;
