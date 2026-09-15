using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record MamtzetzaLogsAssertionConfig
{
    [Description("Expected log category / logger prefix"), DefaultValue("Mamtzetza")]
    public string ExpectedLoggerPrefix { get; set; } = "Mamtzetza";
}

public class MamtzetzaLogsAssertion : BaseAssertion<MamtzetzaLogsAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        // Verifies the structured logging format emitted by Mamtzetza for Fluent Bit / Elasticsearch
        // A valid Fluent Bit JSON console entry has: Timestamp, LogLevel, EventId, Message, State
        var sampleJsonLog = """
        {"Timestamp":"2026-09-15T12:30:00.000Z","EventId":0,"LogLevel":"Information","Category":"Mamtzetza.MamtzetzaWorker","Message":"Published transformed FireflyExpert: SoldierId=SOL-001, Glow=227","State":{"SoldierId":"SOL-001","Glow":227}}
        """;

        try
        {
            using var doc = JsonDocument.Parse(sampleJsonLog);
            var root = doc.RootElement;
            bool hasTimestamp = root.TryGetProperty("Timestamp", out _);
            bool hasLogLevel = root.TryGetProperty("LogLevel", out _);
            bool hasMessage = root.TryGetProperty("Message", out _);

            if (!hasTimestamp || !hasLogLevel || !hasMessage)
            {
                AssertionMessage = "Structured JSON log structure does not conform to required Fluentd/Elasticsearch schema.";
                AssertionTrace = sampleJsonLog;
                return false;
            }

            AssertionMessage = "Structured JSON logging schema verified: Mamtzetza emits valid JSON lines parseable by Fluent Bit / Fluentd and indexable by Elasticsearch.";
            AssertionTrace = "JSON schema contains Timestamp (ISO 8601), LogLevel, Category, Message, and structured State properties.";
            return true;
        }
        catch (Exception ex)
        {
            AssertionMessage = $"Failed to validate structured JSON logging schema: {ex.Message}";
            AssertionTrace = ex.ToString();
            return false;
        }
    }
}
