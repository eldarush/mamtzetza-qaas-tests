using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Soldier = OmegaSolider.Messages.OmegaSolider;
using Expert = OmegaSolider.Messages.FireflyExpert;

namespace OmegaFireflyTests.Assertions;

public record EvidenceBaseline(long StartedAtUnixMs, Dictionary<string, double> Metrics)
{
    public static EvidenceBaseline Load(string path) => JsonSerializer.Deserialize<EvidenceBaseline>(File.ReadAllText(path))
        ?? throw new InvalidDataException("Missing baseline.");
}

public static class EvidenceValidation
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }

    public static void ValidateBatch(IReadOnlyList<Soldier> inputs, IReadOnlyList<Expert> outputs,
        FireflyLogicalAssertionConfig config, long? startedAt = null)
    {
        Require(inputs.Count > 0 && inputs.Count == outputs.Count, "Input/output counts differ or are empty.");
        Require(inputs.All(x => !string.IsNullOrWhiteSpace(x.SoldierId)) &&
            inputs.Select(x => x.SoldierId).Distinct().Count() == inputs.Count, "Input IDs are empty or duplicated.");
        Require(outputs.Select(x => x.SoldierId).Distinct().Count() == outputs.Count, "Duplicate output ID.");
        Require(inputs.Select(x => x.SoldierId).ToHashSet().SetEquals(outputs.Select(x => x.SoldierId)), "Missing or unexpected output ID.");
        var fixtures = LiteralFixtures.Load();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        foreach (var input in inputs)
        {
            var output = outputs.Single(x => x.SoldierId == input.SoldierId);
            foreach (var field in Soldier.Descriptor.Fields.InFieldNumberOrder())
            {
                var actual = Expert.Descriptor.FindFieldByNumber(field.FieldNumber).Accessor.GetValue(output);
                Require(Equals(field.Accessor.GetValue(input), actual), $"{input.SoldierId}: preserved {field.Name} differs.");
            }
            var fixture = fixtures.SingleOrDefault(f => Soldier.Descriptor.Fields.InFieldNumberOrder()
                .Where(field => field.FieldNumber != 1).All(field => Equals(field.Accessor.GetValue(input), field.Accessor.GetValue(f.Input))));
            Require(fixture != null, $"{input.SoldierId}: no reviewed literal fixture; add an explicit expected case.");
            for (var number = 13; number <= 17; number++)
            {
                var field = Expert.Descriptor.FindFieldByNumber(number);
                Require(Equals(field.Accessor.GetValue(fixture!.Expected), field.Accessor.GetValue(output)), $"{input.SoldierId}: {field.Name} differs from literal fixture.");
            }
            var expectedGlow = fixture!.Expected.GlowIntensity + (config.ExpectExternalApiEnabled ? config.ExpectedBonusGlow : 0);
            Require(output.GlowIntensity == expectedGlow, $"{input.SoldierId}: glow expected {expectedGlow}, got {output.GlowIntensity}.");
            var expectedBuff = config.ExpectExternalApiEnabled
                ? $"{config.ExpectedBuffName} - {fixture.Expected.ComedicBuff}" : "Unbuffed Normal Firefly (No API)";
            Require(output.ComedicBuff == expectedBuff, $"{input.SoldierId}: exact buff/title differs.");
            Require(output.ProcessedAtUnixMs >= (startedAt ?? now - 120000) && output.ProcessedAtUnixMs <= now,
                $"{input.SoldierId}: timestamp outside current run.");
        }
    }

    public static Dictionary<string, double> ParseMetrics(string content)
    {
        var result = new Dictionary<string, double>();
        foreach (var raw in content.Split('\n'))
        {
            var line = raw.Trim();
            if (!line.StartsWith("mamtzetza_", StringComparison.Ordinal)) continue;
            var match = Regex.Match(line, @"^(?<key>[a-zA-Z_:][a-zA-Z0-9_:]*(?:\{[^}]*\})?)\s+(?<value>\S+)(?:\s+\d+)?$");
            Require(match.Success, $"Malformed metric sample: {line}");
            var parsed = double.TryParse(match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value);
            Require(parsed && double.IsFinite(value) && value >= 0, $"Invalid metric value: {line}");
            Require(result.TryAdd(match.Groups["key"].Value, value), $"Duplicate metric sample: {line}");
        }
        return result;
    }

    public static void ValidateMetrics(IReadOnlyDictionary<string, double> baseline, IReadOnlyDictionary<string, double> final,
        int count, bool apiEnabled)
    {
        Require(count > 0, "Empty batch.");
        foreach (var (key, expected) in new Dictionary<string, double>
        {
            ["mamtzetza_messages_received_total"] = count,
            ["mamtzetza_messages_processed_total"] = count,
            ["mamtzetza_processing_errors_total"] = 0,
            ["mamtzetza_processing_duration_seconds_count"] = count,
            ["mamtzetza_external_api_calls_total{status=\"success\"}"] = apiEnabled ? count : 0,
            ["mamtzetza_external_api_calls_total{status=\"failure\"}"] = 0
        })
        {
            // Labelled counters are absent until the first use; treat absence as zero only for these labels.
            var optional = key.StartsWith("mamtzetza_external_api_calls_total{", StringComparison.Ordinal);
            Require(optional || (baseline.ContainsKey(key) && final.ContainsKey(key)), $"Missing numeric metric {key}.");
            var before = baseline.TryGetValue(key, out var b) ? b : 0;
            var after = final.TryGetValue(key, out var a) ? a : 0;
            Require(double.IsFinite(before) && double.IsFinite(after) && before >= 0 && after >= before,
                $"Invalid value or reset for {key}.");
            Require(after - before == expected, $"{key}: expected delta {expected}, observed {after - before}.");
        }
        foreach (var key in new[] { "mamtzetza_processing_duration_seconds_sum", "mamtzetza_processing_duration_seconds_bucket{le=\"+Inf\"}" })
            Require(final.ContainsKey(key) && baseline.ContainsKey(key) && final[key] >= baseline[key], $"Missing/reset histogram sample {key}.");
        Require(final["mamtzetza_processing_duration_seconds_bucket{le=\"+Inf\"}"] - baseline["mamtzetza_processing_duration_seconds_bucket{le=\"+Inf\"}"] == count,
            "Histogram +Inf bucket delta differs from batch count.");
    }

    public static void ValidateLogs(string text, IReadOnlyList<Expert> outputs, string prefix, long startedAt, long endedAt)
    {
        Require(outputs.Count > 0, "No output IDs for log correlation.");
        var seen = new HashSet<string>();
        foreach (var line in text.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            JsonDocument document;
            try { document = JsonDocument.Parse(line); }
            catch (JsonException ex) { throw new InvalidDataException("Malformed actual stdout JSON line.", ex); }
            using (document)
            {
                var root = document.RootElement;
                Require(root.TryGetProperty("Timestamp", out var timestamp) &&
                    DateTimeOffset.TryParse(timestamp.GetString()?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _), "Missing/invalid Timestamp.");
                Require(root.TryGetProperty("EventId", out var eventId) && eventId.TryGetInt32(out _), "Missing/invalid EventId.");
                Require(root.TryGetProperty("LogLevel", out var level) && new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" }.Contains(level.GetString()), "Missing/invalid LogLevel.");
                Require(root.TryGetProperty("Category", out var category) && !string.IsNullOrWhiteSpace(category.GetString()), "Missing Category.");
                Require(root.TryGetProperty("Message", out var message) && message.ValueKind == JsonValueKind.String, "Missing Message.");
                Require(root.TryGetProperty("State", out var state) && state.ValueKind == JsonValueKind.Object, "Missing structured State.");
                if (!category.GetString()!.StartsWith(prefix + ".", StringComparison.Ordinal) ||
                    !message.GetString()!.StartsWith("Published transformed FireflyExpert:", StringComparison.Ordinal)) continue;
                Require(state.TryGetProperty("SoldierId", out var id) && id.ValueKind == JsonValueKind.String, "Publication lacks SoldierId.");
                var output = outputs.SingleOrDefault(x => x.SoldierId == id.GetString());
                if (output == null) continue; // Startup / earlier batch lines are excluded by correlation and time.
                var time = DateTimeOffset.Parse(timestamp.GetString()!.Trim(), CultureInfo.InvariantCulture).ToUnixTimeMilliseconds();
                Require(time >= startedAt && time <= endedAt, $"Stale/future log for {id}.");
                Require(state.TryGetProperty("Glow", out var glow) && glow.TryGetInt32(out var value) && value == output.GlowIntensity, $"Wrong/missing Glow for {id}.");
                Require(level.GetString() == "Information" && seen.Add(id.GetString()!), $"Wrong level/duplicate publication log for {id}.");
            }
        }
        Require(seen.SetEquals(outputs.Select(x => x.SoldierId)), "Missing real publication logs for batch IDs or wrong logger prefix.");
    }
}
