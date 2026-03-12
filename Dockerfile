# 1. Build & Restore stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EcoTrack.Core/EcoTrack.Core.csproj", "EcoTrack.Core/"]
COPY ["EcoTrack.Application/EcoTrack.Application.csproj", "EcoTrack.Application/"]
COPY ["EcoTrack.Infrastructure/EcoTrack.Infrastructure.csproj", "EcoTrack.Infrastructure/"]
COPY ["EcoTrack.WebApi/EcoTrack.WebApi.csproj", "EcoTrack.WebApi/"]

RUN dotnet restore "EcoTrack.WebApi/EcoTrack.WebApi.csproj"

COPY . .

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet publish "EcoTrack.WebApi/EcoTrack.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Render/Railway/Heroku 
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
COPY /entrypoint.sh ./entrypoint.sh

# Make entrypoint.sh executable
RUN chmod +x ./entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]
