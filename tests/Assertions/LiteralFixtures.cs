using System.Text.Json;
using Google.Protobuf;
using Soldier = OmegaSolider.Messages.OmegaSolider;
using Expert = OmegaSolider.Messages.FireflyExpert;

namespace OmegaFireflyTests.Assertions;

public record LiteralFixture(Soldier Input, Expert Expected);

public static class LiteralFixtures
{
    // Reviewed literal examples, not another implementation of transformation rules.
    public static IReadOnlyList<LiteralFixture> Load()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "soldiers.json")));
        return json.RootElement.EnumerateArray().Select(row => new LiteralFixture(
            JsonParser.Default.Parse<Soldier>(row.GetProperty("input").GetRawText()),
            JsonParser.Default.Parse<Expert>(row.GetProperty("expected").GetRawText()))).ToList();
    }
}
