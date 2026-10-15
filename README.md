# Mamtzetza QaaS reference suite and teacher solution

This repository tests one isolated **five-message batch**. It is a reference implementation for training, with explicit regression tests for its custom assertions. It does not establish stress, sustained throughput, continuous-stream reliability, or production readiness.

## Reproducible layout and prerequisites

Use .NET SDK **10.0.203**, Docker Desktop with Linux containers and Compose v2, Git, and PowerShell 7 for the helper script. Package references are exact: Runner 4.8.2, Mocker 2.4.7, Common.Assertions/Generators 3.5.6, OmegaSolider 1.1.0. SDK and container versions are pinned in the source; local NuGet packages are in `packages/`. Restores still need nuget.org for dependencies. See restore audit warnings in verification evidence before production use.

Place these repositories side by side (Compose uses `../Mamtzetza`):

```text
workspace/
  OmegaSolider/
  Mamtzetza/
  mamtzetza-qaas-tests/
```

For a delivered bundle, use its included `sources/` directories together. For independent clones, use the exact coordinated commits recorded in the bundle verification report, rather than unrelated default branches.

## Run and reset

From this repository, use a dedicated project so cleanup cannot touch another Compose project:

```powershell
./scripts/Run-Isolated.ps1 -Reset
```

`-Reset` deletes only the `qaas-onboarding` project's containers and volumes, including old evidence and queues. Keep other applications off ports 5672, 15672, 8080 and 9090. The runner waits up to 120 seconds for an active queue consumer, empty queue, both mock HTTP routes, metrics, and actual stdout capture; it then records baseline numeric metrics and start time before publishing. There is no fixed startup sleep. Results are in `allure-results/`; exported baseline and logs are in `evidence/`.

Equivalent commands on Linux/macOS:

```sh
docker compose -p qaas-onboarding down --volumes --remove-orphans
docker compose -p qaas-onboarding up --build --abort-on-container-exit --exit-code-from tests
docker compose -p qaas-onboarding logs --no-color
docker compose -p qaas-onboarding cp tests:/app/evidence/. evidence/
docker compose -p qaas-onboarding down --volumes --remove-orphans
```

## Local development

Use the repository root as working directory so evidence paths resolve consistently. Start RabbitMQ from the isolated Compose project and run the HTTP mocker in a separate terminal:

```powershell
docker compose -p qaas-onboarding up -d rabbitmq
dotnet run --project mocker/MamtzetzaMocker.csproj -- run mocker/mocker.qaas.yaml
```

In another terminal, from this tests repository root, set the API flag and capture path explicitly:

```powershell
$env:ENABLE_EXTERNAL_API = 'true'
$env:EXTERNAL_API_BASE_URL = 'http://127.0.0.1:8080'
$env:LOG_FILE_PATH = Join-Path (Get-Location) 'evidence/mamtzetza.jsonl'
dotnet run --project ../Mamtzetza/src/Mamtzetza/Mamtzetza.csproj
```

Then run `dotnet run --project tests/MamtzetzaTests.csproj -- --isolated tests/test.qaas.yaml` from this root. Reset the isolated RabbitMQ project and restart the worker before each baseline. Do not run local and Compose worker/test modes concurrently. For an API-disabled experiment, set the worker flag false and change **both** logical and metrics assertion configurations to `ExpectExternalApiEnabled: false` in a temporary YAML copy. API success/failure/off and age boundaries are independently covered in Mamtzetza unit tests; the default E2E scenario covers API success.

## Eight reference specifications

| ID | Assertion | Evidence and scope |
|---|---|---|
| 01 | FireflyLogicalAssertion | Exact preservation of all 12 fields, one-to-one IDs, five literal expected derived fields, glow, exact buff/title, current-run timestamp. Unknown fixtures fail explicitly. |
| 02 | OutputDeserializableTo | Binary payload deserialization only. Semantic correctness is checked by 01. |
| 03 | HermeticByExpectedOutputCount | Exactly five outputs; count alone cannot prove identity or no duplicates. |
| 04 | HermeticByInputOutputPercentage | Count ratio 100%; ID correspondence is checked by 01. |
| 05 | DelayByChunks | Five-message batch chunk latency with 10,000 ms configured ceiling. |
| 06 | DelayByAverage | Batch average latency with 5,000 ms configured ceiling. |
| 07 | MamtzetzaMetricsAssertion | Pre/post deltas: received, processed, duration count and +Inf bucket = batch count; processing errors and API failures = 0; API success = count. Reject missing/nonfinite/reset/negative values. API counter counts enrichment attempts, not individual HTTP requests. |
| 08 | MamtzetzaLogsAssertion | Actual flushed stdout file; JSON schema, logger prefix, timestamp, unique SoldierId, expected glow and one publication per output ID. |

[tests_specification.xlsx](tests_specification.xlsx) contains the same eight specifications. Literal expectations live in [soldiers.json](tests/fixtures/soldiers.json). Changing generator inputs requires reviewed expected fixtures; the checker deliberately rejects unreviewed cases instead of reimplementing service rules.

## Unit tests and CI

```powershell
dotnet restore MamtzetzaTests.slnx --configfile NuGet.config
dotnet build MamtzetzaTests.slnx --no-restore -c Release
dotnet test regression/AssertionRegressionTests.csproj --no-build -c Release --logger trx
```

[CI workflow](.github/workflows/ci.yml) builds both QaaS projects, runs assertion regressions, runs the isolated Compose test and uploads logs/baseline/Allure/TRX before cleanup. CI must use the coordinated Mamtzetza commit. The workflow has no package publication, merge or deployment step. Local tests and remote CI are reported separately in the bundle.

## Log capture and optional observability

Mamtzetza's `LOG_FILE_PATH` duplicates its real stdout stream into a flushed file on the shared `run-evidence` volume. This is required for test 08. UTC timestamps are configured at the console formatter. The required training week needs no Elasticsearch administration.

For optional full observability: `docker compose -p qaas-onboarding --profile observability up -d prometheus elasticsearch fluent-bit`. Fluent Bit now tails `/evidence/mamtzetza.jsonl` with the JSON parser and forwards to Elasticsearch; there is no unsupported claim that a bare forward input collects Docker stdout. Inspect `http://localhost:9200/mamtzetza-logs/_search?pretty` after processing. Prometheus is at `http://localhost:9091`; raw worker metrics are at `http://localhost:9090/metrics`. Optional profile validation is reported separately.
