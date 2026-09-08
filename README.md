# Omega Firefly QaaS Test Suite & Mocker Guide

Welcome to the **QaaS** (Quality-as-a-Service) educational project! This repository demonstrates how to build, mock, and test distributed event-driven systems using the latest **QaaS Runner** and **QaaS Mocker** packages.

---

## 1. What is QaaS?

**QaaS** is an enterprise test automation and verification framework designed for distributed, event-driven, and asynchronous systems (RabbitMQ, Kafka, SQS, HTTP, gRPC, etc.).

Instead of writing brittle end-to-end glue scripts, QaaS allows you to model testing workflows declaratively (via YAML or C# Code-as-Configuration) into:
- **DataSources & Generators**: Modules that generate or load synthetic or production-like test datasets.
- **Sessions & Staged Actions**: Orchestrated execution workflows that publish to queues, consume from topics, make HTTP calls, and probe infrastructure.
- **Assertions**: Statistical, hermetic, performance, and logical evaluators that verify system behavior.
- **Mocker**: Standalone or embedded server instances capable of mocking out external HTTP, gRPC, or socket APIs with customizable stubs and dynamic transaction processors.

---

## 2. System Architecture

```mermaid
flowchart TD
    subgraph QaaS Runner [QaaS Test Process]
        GEN["OmegaSoliderGenerator<br/>(Custom Generator)"] -->|Generate Protobuf Data| PUB["RabbitMQ Publisher<br/>(Exchange: omega-solider-input)"]
        SUB["RabbitMQ Consumer<br/>(Exchange: firefly-expert-output)"] -->|Deserialize FireflyExpert| DESER["ProtobufMessage Deserializer"]
        
        PUB -.->|Record Inputs| SESS_DATA[("SessionData<br/>(Inputs & Outputs)")]
        DESER -.->|Record Outputs| SESS_DATA
        
        SESS_DATA --> A1["HermeticByInputOutputPercentage<br/>(Validates 100% Throughput)"]
        SESS_DATA --> A2["DelayByChunks<br/>(Validates Latency SLA < 10s)"]
        SESS_DATA --> A3["FireflyLogicalAssertion<br/>(Custom Logical Assertion)"]
    end

    subgraph Messaging [RabbitMQ Broker :5672]
        PUB -->|BasicPublish| EX_IN[(Exchange: omega-solider-input)]
        EX_IN --> Q_IN[(Queue: omega-solider-input-queue)]
        Q_OUT[(Queue: firefly-expert-output-queue)] --> EX_OUT[(Exchange: firefly-expert-output)]
        EX_OUT --> SUB
    end

    subgraph Target System [OmegaFireflyComponent]
        Q_IN --> CONSUMER["OmegaFireflyWorker"]
        CONSUMER --> TRANSFORM["FireflyTransformer"]
        TRANSFORM --> PUBLISHER["Publish to RabbitMQ"]
        PUBLISHER --> Q_OUT
    end

    subgraph QaaS Mocker [QaaS Mocker HTTP Server :8080]
        TRANSFORM -.->|GET /api/v1/buff/{soldierId}| MOCK_GET["BuffProcessor"]
        TRANSFORM -.->|POST /api/v1/funny-title| MOCK_POST["FunnyTitleProcessor"]
        MOCK_GET -.->|JSON Buff| TRANSFORM
        MOCK_POST -.->|JSON Title| TRANSFORM
    end
```

---

## 3. Teaching Modules

### Module A: Custom Generators (`OmegaSoliderGenerator`)
A **Generator** produces data items passed into publishers. To write a custom generator:
1. Inherit from `BaseGenerator<TConfiguration>` where `TConfiguration` is a POCO with a parameterless constructor.
2. Override `IEnumerable<Data<object>> Generate(...)`:

```csharp
public class OmegaSoliderGenerator : BaseGenerator<OmegaSoliderGeneratorConfig>
{
    public override IEnumerable<Data<object>> Generate(
        IImmutableList<SessionData> sessionDataList,
        IImmutableList<DataSource> dataSourceList)
    {
        for (int i = 1; i <= Configuration.Count; i++)
        {
            var soldier = new OmegaSolider.Messages.OmegaSolider
            {
                SoldierId = $"{Configuration.IdPrefix}{i:D3}",
                Codename = $"Bravo-{i}",
                RankLevel = (i % 5) + 1,
                BraveryPoints = i * 15,
                FavoriteSnack = Configuration.FavoriteSnack
            };

            yield return new Data<object>
            {
                Body = soldier.ToByteArray()
            };
        }
    }
}
```

In your YAML configuration, reference your generator by class name:
```yaml
DataSources:
  - Name: GeneratedOmegaSoldiers
    Generator: OmegaSoliderGenerator
    GeneratorConfiguration:
      Count: 5
      IdPrefix: "SOL-"
      FavoriteSnack: "Quantum Doritos"
```

---

### Module B: Consuming and Deserializing Protobuf Messages
In `test.qaas.yaml`, QaaS configures a RabbitMQ consumer and tells the deserializer to parse byte buffers into the strongly-typed C# Protobuf class:

```yaml
Consumers:
  - Name: FireflyConsumer
    TimeoutMs: 15000
    RabbitMq:
      Host: 127.0.0.1
      Port: 5672
      Username: admin
      Password: admin
      ExchangeName: firefly-expert-output
      RoutingKey: /
    Deserialize:
      Deserializer: ProtobufMessage
      SpecificType:
        AssemblyName: OmegaSolider
        TypeFullName: OmegaSolider.Messages.FireflyExpert
```

---

### Module C: Assertions

#### 1. Hermetic Throughput Assertion (`HermeticByInputOutputPercentage`)
Ensures no messages were dropped, duplicated, or lost in transit:
```yaml
Assertions:
  - Name: HermeticThroughput
    Assertion: HermeticByInputOutputPercentage
    SessionNames: [OmegaToFireflyRoundTrip]
    AssertionConfiguration:
      InputNames: [SoldierPublisher]
      OutputNames: [FireflyConsumer]
      ExpectedPercentage: 100
```

#### 2. Delay Assertion (`DelayByChunks`)
Measures the time elapsed between publication and consumption:
```yaml
  - Name: MaxProcessingDelay
    Assertion: DelayByChunks
    SessionNames: [OmegaToFireflyRoundTrip]
    AssertionConfiguration:
      Input:
        Name: SoldierPublisher
        ChunkSize: 1
      Output:
        Name: FireflyConsumer
        ChunkSize: 1
      MaximumDelayMs: 10000
```

#### 3. Custom Logical Assertion (`FireflyLogicalAssertion`)
Inherits from `BaseAssertion<TConfiguration>`. Inspects the published and consumed payloads stored in `SessionData`:
```csharp
public class FireflyLogicalAssertion : BaseAssertion<FireflyLogicalAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var session = sessionDataList.Single();
        // Correlate inputs and outputs by SoldierId
        // Validate formula: Glow = (RankLevel * 10) + (BraveryPoints * 2) + BonusGlow
        // Validate external comedic buff received from Mocker
        return true;
    }
}
```

---

### Module D: QaaS Mocker (HTTP Mock Server)
To mock external services during tests, **QaaS Mocker** is configured via `mocker.qaas.yaml`.

This suite mocks two distinct endpoints using two different HTTP methods:
1. `GET /api/v1/buff/{soldierId}` $\to$ Handled by `BuffProcessor`
2. `POST /api/v1/funny-title` $\to$ Handled by `FunnyTitleProcessor`

```yaml
Servers:
  - Http:
      Port: 8080
      IsLocalhost: false
      Endpoints:
        - Path: /api/v1/buff/{soldierId}
          Actions:
            - Name: GetSoldierBuff
              Method: Get
              TransactionStubName: BuffStub

        - Path: /api/v1/funny-title
          Actions:
            - Name: PostFunnyTitle
              Method: Post
              TransactionStubName: FunnyTitleStub
```

---

## 4. Running the Complete Suite

### Option 1: One-Command Verification with Docker Compose (Recommended)

Run everything (RabbitMQ, Mocker, Demo Component, and Test Runner):

```bash
docker compose up --build --abort-on-container-exit
```

### Option 2: Running Locally

#### Step 1: Start RabbitMQ
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin rabbitmq:3-management
```

#### Step 2: Start QaaS Mocker
```bash
dotnet run --project src/OmegaFireflyMocker/OmegaFireflyMocker.csproj -- run mocker.qaas.yaml
```

#### Step 3: Start Demo Component
In the `omega-firefly-component` repository:
```bash
ENABLE_EXTERNAL_API=true EXTERNAL_API_BASE_URL=http://127.0.0.1:8080 dotnet run --project src/OmegaFireflyComponent/OmegaFireflyComponent.csproj
```

#### Step 4: Run QaaS Tests
```bash
dotnet run --project src/OmegaFireflyTests/OmegaFireflyTests.csproj -- run test.qaas.yaml
```

---

## 5. Package Versions Used

- **`QaaS.Runner`**: `4.8.2`
- **`QaaS.Mocker`**: `2.4.7`
- **`QaaS.Common.Assertions`**: `3.5.6`
- **`QaaS.Common.Generators`**: `3.5.6`
- **`OmegaSolider`**: `1.0.0`
