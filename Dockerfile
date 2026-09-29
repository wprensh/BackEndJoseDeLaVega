# Imagen de producción de la API (.NET 9). La usa Render para desplegar el servicio.

# ---------- Compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS compilacion
WORKDIR /src

# Primero solo los proyectos: la restauración de paquetes queda en caché si no cambian.
COPY Directory.Build.props ./
COPY src/JoseDeLaVega.Domain/JoseDeLaVega.Domain.csproj src/JoseDeLaVega.Domain/
COPY src/JoseDeLaVega.Application/JoseDeLaVega.Application.csproj src/JoseDeLaVega.Application/
COPY src/JoseDeLaVega.Infrastructure/JoseDeLaVega.Infrastructure.csproj src/JoseDeLaVega.Infrastructure/
COPY src/JoseDeLaVega.Api/JoseDeLaVega.Api.csproj src/JoseDeLaVega.Api/
RUN dotnet restore src/JoseDeLaVega.Api/JoseDeLaVega.Api.csproj

COPY src/ src/
# Las advertencias se revisan en desarrollo; aquí no deben frenar un despliegue si cambia la versión del SDK.
RUN dotnet publish src/JoseDeLaVega.Api/JoseDeLaVega.Api.csproj -c Release -o /app --no-restore -p:TreatWarningsAsErrors=false

# ---------- Ejecución ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=compilacion /app ./

# Usuario sin privilegios que trae la imagen oficial de .NET.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "JoseDeLaVega.Api.dll"]
