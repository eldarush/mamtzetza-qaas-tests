param([switch]$E2E)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$labRoot = $PSScriptRoot
if (-not (Test-Path -LiteralPath (Join-Path $labRoot 'Mamtzetza/src/Mamtzetza/FireflyTransformer.cs'))) {
    throw 'Run the copy inside student/lab after the teacher prepares the assessment.'
}
Push-Location $labRoot
try {
    if ($E2E) {
        Push-Location 'mamtzetza-qaas-tests'
        try {
            # Compose resolves ../Mamtzetza to the seeded sibling inside this lab.
            docker compose -p qaas-assessment down --volumes --remove-orphans
            if ($LASTEXITCODE -ne 0) { throw 'Assessment project reset failed.' }
            docker compose -p qaas-assessment up --build --abort-on-container-exit --exit-code-from tests
            $testExit = $LASTEXITCODE
            New-Item -ItemType Directory -Force -Path evidence | Out-Null
            docker compose -p qaas-assessment logs --no-color | Set-Content evidence/compose.log
            docker compose -p qaas-assessment cp tests:/app/evidence/. evidence/
            if ($LASTEXITCODE -ne 0) { throw 'Assessment evidence export failed.' }
        } finally {
            docker compose -p qaas-assessment down --volumes --remove-orphans
            Pop-Location
        }
        exit $testExit
    }
    dotnet restore Mamtzetza/Mamtzetza.slnx --configfile Mamtzetza/NuGet.config --locked-mode
    if ($LASTEXITCODE -ne 0) { throw 'Assessment service restore failed.' }
    dotnet restore Starter/Starter.csproj --configfile Mamtzetza/NuGet.config
    if ($LASTEXITCODE -ne 0) { throw 'Assessment starter restore failed.' }
    dotnet test Mamtzetza/Mamtzetza.slnx -c Release --no-restore
    $serviceExit = $LASTEXITCODE
    dotnet test Starter/Starter.csproj -c Release --no-restore
    $starterExit = $LASTEXITCODE
    if ($serviceExit -ne 0 -or $starterExit -ne 0) { exit 1 }
    exit 0
} finally { Pop-Location }
