# =============================================================================
# File    : Dockerfile
# Purpose : Builds and runs the CardioSense API container (Domain + Application + Infrastructure + Api).
# Logic   : Two stages. The build stage restores packages from the .csproj files alone first, so Docker caches the
#           (slow) restore until a project file or a package version changes; only then is the source copied. The
#           runtime stage is the small ASP.NET image running as the image's non-root user.
# Usage   : docker build -t cardiosense-api .      (context = this folder, the solution root)
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/CardioSense.Domain/CardioSense.Domain.csproj src/CardioSense.Domain/
COPY src/CardioSense.Application/CardioSense.Application.csproj src/CardioSense.Application/
COPY src/CardioSense.Infrastructure/CardioSense.Infrastructure.csproj src/CardioSense.Infrastructure/
COPY src/CardioSense.Api/CardioSense.Api.csproj src/CardioSense.Api/
RUN dotnet restore src/CardioSense.Api/CardioSense.Api.csproj

COPY src/ src/
RUN dotnet publish src/CardioSense.Api/CardioSense.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Port 8080 is the default of .NET container images; TZ makes the patient "hourly timeline" use Cairo time.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    TZ=Africa/Cairo
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "CardioSense.Api.dll"]
