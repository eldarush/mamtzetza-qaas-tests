# Mamtzetza Reference QaaS Test Suite & Mocker

> **Working Tests Example & Teacher's Solution Key**  
> This repository is the official reference implementation for testing the [Mamtzetza](https://github.com/eldarush/Mamtzetza) microservice using the **QaaS (Quality as a Service)** ecosystem (`QaaS.Runner 4.8.2`, `QaaS.Mocker 2.4.7`, `QaaS.Common.Assertions 3.5.6`, `QaaS.Common.Generators 3.5.6`).

---

## 🎯 Purpose of this Repository

This is an **executable, production-ready reference test suite** that actively tests and validates that the `Mamtzetza` microservice works correctly in an event-driven, observable containerized environment.

As an instructor, you can use this repository as a **golden reference / answer key** to compare student submissions and suggestions against. Wherever possible, the test suite leverages official built-in hooks from **`QaaS.Common.Assertions`** (such as `OutputDeserializableTo`, `HermeticByInputOutputPercentage`, `HermeticByExpectedOutputCount`, `DelayByChunks`, and `DelayByAverage`), keeping custom hooks to the absolute required minimum.

Included in the root directory is **`tests_specification.xlsx`**, an Excel spreadsheet detailing all 8 test specifications:
`Test Type, Input, Expected output, Test Description, Notes`

---

## 📊 Summary of the 8 Reference Tests

| Test ID | Test Name | Hook Type | Assertion Hook | Description |
| :--- | :--- | :--- | :--- | :--- |
| **TEST-01** | `TransformationBusinessLogic` | **Custom Hook** | `FireflyLogicalAssertion` | Verifies all 12 input soldier fields are preserved and the 5 specialization fields are deterministically computed (`favorite_technology`, `favorite_team`, `favorite_commander`, `favorite_woman`, `favorite_coding_language`), base glow, and mock comedic buff. |
| **TEST-02** | `OutputContractValidation` | **QaaS.Common.Assertions** | `OutputDeserializableTo` | Validates wire contract compliance, asserting that consumed RabbitMQ payloads successfully deserialize into the `OmegaSolider.Messages.FireflyExpert` Protobuf contract. |
| **TEST-03** | `ExpectedBatchOutputCount` | **QaaS.Common.Assertions** | `HermeticByExpectedOutputCount` | Asserts exact batch output count matching (5 messages received) without queue truncation or buffer drops. |
| **TEST-04** | `HermeticThroughput` | **QaaS.Common.Assertions** | `HermeticByInputOutputPercentage` | Asserts 100% input/output message delivery across RabbitMQ exchanges and queues with zero message loss. |
| **TEST-05** | `MaxProcessingDelayByChunks` | **QaaS.Common.Assertions** | `DelayByChunks` | Asserts chunk-by-chunk transit latency strictly conforms to the < 10,000ms SLA. |
| **TEST-06** | `ProcessingDelayByAverage` | **QaaS.Common.Assertions** | `DelayByAverage` | Measures and enforces average round-trip transit processing latency under 5,000ms. |
| **TEST-07** | `MetricsObservabilityEndpoint` | **Custom Hook** | `MamtzetzaMetricsAssertion` | Scrapes `http://<host>:9090/metrics` via HTTP and validates Prometheus exposition format with active message counters. |
| **TEST-08** | `StructuredJsonLogsCompliance` | **Custom Hook** | `MamtzetzaLogsAssertion` | Verifies single-line structured JSON logs (`Timestamp`, `LogLevel`, `Category`, `Message`, `State`) formatted for Fluent Bit / Fluentd and Elasticsearch ingestion. |

---

## 🏗 Repository Structure

```
mamtzetza-qaas-tests/
├── tests_specification.xlsx         # 📑 Excel specification describing the 8 tests
├── docker-compose.yml               # 🐳 Complete stack: RabbitMQ, Mocker, Mamtzetza, Prometheus, Elastic, Fluent-Bit, Tests
├── NuGet.config                     # Configured for local packages + nuget.org
│
├── mocker/                          # 🎭 QaaS.Mocker HTTP Server
│   ├── Processors/
│   │   ├── BuffProcessor.cs         # GET /api/v1/buff/{soldierId}
│   │   └── FunnyTitleProcessor.cs   # POST /api/v1/funny-title (supports favoriteFood & favoriteSnack)
│   ├── Dockerfile                   # Builds mamtzetza-mocker container
│   ├── MamtzetzaMocker.csproj       # .NET 10 project using QaaS.Mocker 2.4.7
│   ├── Program.cs                   # Mocker host startup
│   └── mocker.qaas.yaml             # Declarative routes & stubs configuration
│
├── tests/                           # 🧪 QaaS.Runner Test Suite
│   ├── Assertions/
│   │   ├── FireflyLogicalAssertion.cs       # Custom: 12 preserved + 5 derived fields + base glow + buff
│   │   ├── MamtzetzaMetricsAssertion.cs      # Custom: Scrapes Prometheus /metrics on port 9090
│   │   └── MamtzetzaLogsAssertion.cs         # Custom: Validates structured JSON logging schema
│   │   # (Built-in assertions OutputDeserializableTo, HermeticByInputOutputPercentage,
│   │   #  HermeticByExpectedOutputCount, DelayByChunks, DelayByAverage imported via QaaS.Common.Assertions)
│   ├── Generators/
│   │   └── OmegaSoliderGenerator.cs         # Generates diverse Protobuf archetypes
│   ├── Dockerfile                   # Builds mamtzetza-runner-tests container
│   ├── MamtzetzaTests.csproj        # .NET 10 project using QaaS.Runner 4.8.2
│   ├── Program.cs                   # Runner CLI entrypoint
│   ├── test.qaas.yaml               # Test session for local host execution (127.0.0.1)
│   └── test-docker.qaas.yaml        # Test session for Docker network execution (rabbitmq, mamtzetza)
│
├── observability/                   # 📈 Observability Stack Configurations
│   ├── prometheus.yml               # Scrape config for Mamtzetza metrics (port 9090)
│   └── fluent-bit.conf              # Fluent Bit forwarder shipping JSON logs to Elasticsearch
│
└── packages/                        # 📦 Local Protobuf NuGet package
    └── OmegaSolider.1.1.0.nupkg     # Protobuf contracts for OmegaSolider & FireflyExpert
```

---

## 📜 Protobuf Schema & Calculated Fields

### 1. `OmegaSolider` (Input)
- `soldier_id` (`string`)
- `name` (`string`)
- `rank` (`string`)
- `age` (`int32`)
- `favorite_food` (`string`)
- `favorite_tv_show` (`string`)
- `shoe_size` (`float`)
- `height` (`float`)
- `weight` (`float`)
- `lucky_number` (`int32`)
- `hobby` (`string`)
- `origin_planet` (`string`)

### 2. `FireflyExpert` (Output)
Contains **all 12 input fields** above, plus:
- **`favorite_technology`**: Derived from `favorite_tv_show` (e.g. Star Trek/Wars -> `"Antimatter Warp Core"`, Expanse -> `"Epstein Fusion Drive"`, Matrix -> `"Neural Direct Link"`, Doctor Who -> `"TARDIS Chrono-Engine"`, Cyberpunk -> `"Sandevistan Neural Implant"`).
- **`favorite_team`**: Derived from `origin_planet` (e.g. Mars -> `"Martian Dust Devils"`, Earth -> `"Terran Cyber Knights"`, Jupiter -> `"Great Red Spot Cyclones"`).
- **`favorite_commander`**: Derived from `rank` (e.g. General/Commander -> `"General Kenobi"`, Captain -> `"Captain Jean-Luc Picard"`, Sergeant/Major -> `"Sergeant Avery Johnson"`).
- **`favorite_woman`**: Derived from `|lucky_number| % 5` (0 -> `"Ada Lovelace"`, 1 -> `"Marie Curie"`, 2 -> `"Grace Hopper"`, 3 -> `"Margaret Hamilton"`, 4 -> `"Hedy Lamarr"`).
- **`favorite_coding_language`**: Derived from `age` (<25 -> `"Rust"`, 25-34 -> `"C#"`, 35-44 -> `"Python"`, 45-54 -> `"C++"`, >=55 -> `"LISP"`).
- **`glow_intensity`**: `(int)(height + weight * 0.5f) + (|lucky_number| % 10) + bonusGlow`.
- **`comedic_buff`**: `"{buffName} - {title}"` when API is enabled, or graceful fallback.
- **`processed_at_unix_ms`**: Unix epoch timestamp in milliseconds.

---

## 🚀 How to Run Everything

### Option 1: Complete Stack via Docker Compose (Recommended)

To run the entire system — RabbitMQ, Mocker, Mamtzetza, Prometheus, and the automated QaaS test suite:

```bash
docker compose up --build --abort-on-container-exit --exit-code-from tests
```

#### What happens during execution:
1. **RabbitMQ**: Spins up on ports `5672` and `15672` with healthcheck.
2. **QaaS Mocker**: Listens on port `8080`, mocking GET `/api/v1/buff/{id}` and POST `/api/v1/funny-title`.
3. **Mamtzetza**: Connects to RabbitMQ, starts consuming from `omega-solider-input-queue`, emits single-line JSON logs to stdout, and exposes Prometheus metrics on port `9090`.
4. **Prometheus**: Automatically scrapes `http://mamtzetza:9090/metrics` every 5 seconds on host port `9091`.
5. **QaaS Test Runner**: Generates sample soldier records, publishes them, consumes the output, executes all 8 assertions (including `OutputDeserializableTo`, `DelayByChunks`, `DelayByAverage`, `HermeticByInputOutputPercentage`, `HermeticByExpectedOutputCount`), and exits with code `0`.

---

### Option 2: Running Observability Stack (Prometheus, Elasticsearch & Fluent Bit)

The `docker-compose.yml` includes full observability services:

1. **Start all infrastructure & observability containers**:
   ```bash
   docker compose up -d rabbitmq mocker mamtzetza prometheus elasticsearch fluent-bit
   ```

2. **Verify Prometheus Metrics**:
   - Open your browser to `http://localhost:9091` to explore Prometheus.
   - Search for `mamtzetza_messages_received_total` or `mamtzetza_processing_duration_seconds_bucket`.
   - Direct raw metrics endpoint: `http://localhost:9090/metrics`.

3. **Verify Centralized JSON Logs in Elasticsearch**:
   - Check Elasticsearch cluster health:
     ```bash
     curl -s http://localhost:9200/_cluster/health
     ```
   - Check indexed Mamtzetza log documents:
     ```bash
     curl -s http://localhost:9200/mamtzetza-logs/_search?pretty
     ```
   - Fluent Bit collects stdout JSON logs directly and indexes them under index `mamtzetza-logs`.

4. **Run the tests against the live stack**:
   ```bash
   docker compose run --rm tests sh -c "dotnet MamtzetzaTests.dll run test-docker.qaas.yaml"
   ```

---

### Option 3: Local CLI Run (Step-by-Step)

If developing locally without running tests inside a container:

1. **Start RabbitMQ**:
   ```bash
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin rabbitmq:3-management
   ```

2. **Start QaaS HTTP Mocker**:
   ```bash
   cd mamtzetza-qaas-tests
   dotnet run --project mocker/MamtzetzaMocker.csproj -- run mocker/mocker.qaas.yaml
   ```

3. **Start Mamtzetza**:
   ```bash
   cd ../Mamtzetza
   dotnet run --project src/Mamtzetza/Mamtzetza.csproj
   ```

4. **Run the 8 QaaS Tests**:
   ```bash
   cd ../mamtzetza-qaas-tests/tests
   dotnet run --project MamtzetzaTests.csproj -- run test.qaas.yaml
   ```
