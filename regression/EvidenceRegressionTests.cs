using System.Text.Json;
using Google.Protobuf;
using OmegaFireflyTests.Assertions;
using Xunit;
using Expert = OmegaSolider.Messages.FireflyExpert;

public class EvidenceRegressionTests
{
    private static Expert Output()
    {
        var row = LiteralFixtures.Load()[0];
        var output = Expert.Parser.ParseFrom(row.Input.ToByteArray());
        output.MergeFrom(row.Expected);
        output.GlowIntensity += 50;
        output.ComedicBuff = "Quantum Disco Sparkles - Supreme Commander of Shawarma";
        output.ProcessedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return output;
    }

    [Fact]
    public void ValidLiteralBatchPasses() => EvidenceValidation.ValidateBatch([LiteralFixtures.Load()[0].Input], [Output()], new());

    [Theory]
    [InlineData("title")]
    [InlineData("timestamp")]
    [InlineData("preserved")]
    [InlineData("derived")]
    [InlineData("id")]
    [InlineData("glow")]
    public void MutatedOutputFails(string mutation)
    {
        var output = Output();
        switch (mutation)
        {
            case "title": output.ComedicBuff = "Quantum Disco Sparkles - WRONG"; break;
            case "timestamp": output.ProcessedAtUnixMs = 1; break;
            case "preserved": output.Name = "wrong"; break;
            case "derived": output.FavoriteTechnology = "wrong"; break;
            case "id": output.SoldierId = "unexpected"; break;
            case "glow": output.GlowIntensity++; break;
        }
        Assert.Throws<InvalidDataException>(() => EvidenceValidation.ValidateBatch([LiteralFixtures.Load()[0].Input], [output], new()));
    }

    [Fact]
    public void EqualCountsCanHideLossAndDuplication()
    {
        var a = LiteralFixtures.Load()[0].Input;
        var b = a.Clone(); b.SoldierId = "different";
        Assert.Throws<InvalidDataException>(() => EvidenceValidation.ValidateBatch([a, b], [Output(), Output()], new()));
    }

    private static Dictionary<string, double> Metrics(int count) => new()
    {
        ["mamtzetza_messages_received_total"] = count,
        ["mamtzetza_messages_processed_total"] = count,
        ["mamtzetza_processing_errors_total"] = 0,
        ["mamtzetza_processing_duration_seconds_count"] = count,
        ["mamtzetza_processing_duration_seconds_sum"] = count * 0.1,
        ["mamtzetza_processing_duration_seconds_bucket{le=\"+Inf\"}"] = count,
        ["mamtzetza_external_api_calls_total{status=\"success\"}"] = count
    };

    [Fact]
    public void RealNumericDeltasPass() => EvidenceValidation.ValidateMetrics(Metrics(7), Metrics(12), 5, true);

    [Theory]
    [InlineData("# mamtzetza_messages_received_total\n# mamtzetza_messages_processed_total")]
    [InlineData("mamtzetza_messages_received_total NaN")]
    [InlineData("mamtzetza_messages_received_total -1")]
    [InlineData("mamtzetza_messages_received_total 0\nmamtzetza_messages_received_total 0")]
    public void MetricNamesAloneAndInvalidNumbersFail(string scrape)
    {
        Assert.Throws<InvalidDataException>(() => EvidenceValidation.ValidateMetrics(Metrics(0), EvidenceValidation.ParseMetrics(scrape), 5, true));
    }

    [Theory]
    [InlineData("mamtzetza_messages_processed_total", 4)]
    [InlineData("mamtzetza_processing_errors_total", 1)]
    [InlineData("mamtzetza_processing_duration_seconds_count", 0)]
    [InlineData("mamtzetza_external_api_calls_total{status=\"success\"}", 0)]
    [InlineData("mamtzetza_messages_received_total", 99)]
    public void WrongMetricDeltaFails(string key, double value)
    {
        var final = Metrics(5); final[key] = value;
        Assert.Throws<InvalidDataException>(() => EvidenceValidation.ValidateMetrics(Metrics(0), final, 5, true));
    }

    private static string Log(Expert output) => JsonSerializer.Serialize(new
    {
        Timestamp = DateTimeOffset.UtcNow.ToString("O"), EventId = 0, LogLevel = "Information",
        Category = "Mamtzetza.MamtzetzaWorker",
        Message = $"Published transformed FireflyExpert: SoldierId={output.SoldierId}, Glow={output.GlowIntensity}",
        State = new { output.SoldierId, Glow = output.GlowIntensity }
    });

    [Fact]
    public void ValidActualLogPasses()
    {
        var output = Output();
        EvidenceValidation.ValidateLogs(Log(output), [output], "Mamtzetza", 0, long.MaxValue);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("malformed")]
    [InlineData("category")]
    [InlineData("timestamp")]
    [InlineData("glow")]
    [InlineData("id")]
    [InlineData("duplicate")]
    public void BrokenLoggingFails(string mutation)
    {
        var output = Output();
        var log = Log(output);
        log = mutation switch
        {
            "empty" => "", "malformed" => "not-json", "category" => log.Replace("Mamtzetza.MamtzetzaWorker", "Wrong.Worker"),
            "timestamp" => log.Replace("Timestamp", "MissingTimestamp"), "glow" => log.Replace("\"Glow\":261", "\"Glow\":0"),
            "id" => log.Replace("SOL-001", "WRONG"), "duplicate" => log + "\n" + log, _ => log
        };
        Assert.Throws<InvalidDataException>(() => EvidenceValidation.ValidateLogs(log, [output], "Mamtzetza", 0, long.MaxValue));
    }

    [Fact]
    public void StaleLogFails()
    {
        var output = Output();
        Assert.Throws<InvalidDataException>(() => EvidenceValidation.ValidateLogs(Log(output), [output], "Mamtzetza", long.MaxValue, long.MaxValue));
    }
}
