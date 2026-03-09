# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY EcoTrack.Core/EcoTrack.Core.csproj EcoTrack.Core/
COPY EcoTrack.Application/EcoTrack.Application.csproj EcoTrack.Application/
COPY EcoTrack.Infrastructure/EcoTrack.Infrastructure.csproj EcoTrack.Infrastructure/
COPY EcoTrack.WebApi/EcoTrack.WebApi.csproj EcoTrack.WebApi/
RUN dotnet restore EcoTrack.WebApi/EcoTrack.WebApi.csproj

# Copy everything and build
COPY . .
WORKDIR /src/EcoTrack.WebApi
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /app/publish .
# Use runtime shell expansion so Railway PORT is honored; fallback to 8080 for local runs.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet EcoTrack.WebApi.dll"]
