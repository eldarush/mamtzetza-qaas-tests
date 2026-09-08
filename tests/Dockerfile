FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NuGet.config .
COPY packages/ packages/
COPY tests/ tests/

RUN dotnet restore tests/MamtzetzaTests.csproj --configfile NuGet.config
RUN dotnet publish tests/MamtzetzaTests.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MamtzetzaTests.dll", "run", "test-docker.qaas.yaml"]
