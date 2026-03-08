FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

COPY ["EcoTrack.slnx", "./"]
COPY ["EcoTrack.Core/EcoTrack.Core.csproj", "EcoTrack.Core/"]
COPY ["EcoTrack.Application/EcoTrack.Application.csproj", "EcoTrack.Application/"]
COPY ["EcoTrack.Infrastructure/EcoTrack.Infrastructure.csproj", "EcoTrack.Infrastructure/"]
COPY ["EcoTrack.WebApi/EcoTrack.WebApi.csproj", "EcoTrack.WebApi/"]

RUN dotnet restore "EcoTrack.WebApi/EcoTrack.WebApi.csproj"

FROM restore AS publish-aot
WORKDIR /src

RUN apt-get update \
    && apt-get install -y --no-install-recommends clang zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

COPY . .
RUN dotnet publish "EcoTrack.WebApi/EcoTrack.WebApi.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    /p:PublishAot=true \
    /p:InvariantGlobalization=true \
    /p:StripSymbols=true \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish-aot /app/publish .
ENTRYPOINT ["./EcoTrack.WebApi"]
