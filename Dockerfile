# syntax=docker/dockerfile:1.7
# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first — copy only csproj files so the restore layer is cacheable
# and only rebuilt when package references change.
COPY src/CarLookup.Core/CarLookup.Core.csproj src/CarLookup.Core/
COPY src/CarLookup.Web/CarLookup.Web.csproj  src/CarLookup.Web/
RUN dotnet restore src/CarLookup.Web/CarLookup.Web.csproj

# Copy sources and publish a framework-dependent release build.
COPY src/ src/
RUN dotnet publish src/CarLookup.Web/CarLookup.Web.csproj \
        --configuration Release \
        --output /app/publish \
        --no-restore \
        /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# The aspnet:9.0 base image ships a non-root `app` user and exposes 8080 by default;
# we set these explicitly for clarity.
COPY --from=build --chown=app:app /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "CarLookup.Web.dll"]
