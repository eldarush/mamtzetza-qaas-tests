param([switch]$Reset)
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    if (-not (Test-Path -LiteralPath '../Mamtzetza/src/Mamtzetza/Mamtzetza.csproj')) { throw 'Place Mamtzetza beside mamtzetza-qaas-tests; see README.' }
    docker version --format '{{.Server.Version}}'
    if ($LASTEXITCODE -ne 0) { throw 'Start Docker Desktop with Linux containers, then retry.' }
    if ($Reset) {
        docker compose -p qaas-onboarding down --volumes --remove-orphans
        if ($LASTEXITCODE -ne 0) { throw 'Isolated reset failed.' }
    }
    docker compose -p qaas-onboarding up --build --abort-on-container-exit --exit-code-from tests
    $testExit = $LASTEXITCODE
    New-Item -ItemType Directory -Force -Path evidence | Out-Null
    docker compose -p qaas-onboarding logs --no-color | Set-Content evidence/compose.log
    docker compose -p qaas-onboarding cp tests:/app/evidence/. evidence/
    if ($LASTEXITCODE -ne 0) { throw 'Evidence export failed.' }
    if ($testExit -ne 0) { throw "QaaS run failed with exit code $testExit. See evidence and allure-results." }
} finally { Pop-Location }
