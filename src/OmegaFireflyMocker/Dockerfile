FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NuGet.config .
COPY packages/ packages/
COPY src/OmegaFireflyMocker/ src/OmegaFireflyMocker/

RUN dotnet restore src/OmegaFireflyMocker/OmegaFireflyMocker.csproj --configfile NuGet.config
RUN dotnet publish src/OmegaFireflyMocker/OmegaFireflyMocker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "OmegaFireflyMocker.dll", "run", "mocker.qaas.yaml"]
