#!/bin/sh
set -e

dotnet tool install --global dotnet-ef || true
export PATH="$PATH:/root/.dotnet/tools"

dotnet ef database update --project EcoTrack.Infrastructure --startup-project EcoTrack.WebApi || true

ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet EcoTrack.WebApi.dll

