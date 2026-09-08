FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NuGet.config .
COPY packages/ packages/
COPY src/OmegaFireflyTests/ src/OmegaFireflyTests/

RUN dotnet restore src/OmegaFireflyTests/OmegaFireflyTests.csproj --configfile NuGet.config
RUN dotnet publish src/OmegaFireflyTests/OmegaFireflyTests.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OmegaFireflyTests.dll", "run", "test-docker.qaas.yaml"]
