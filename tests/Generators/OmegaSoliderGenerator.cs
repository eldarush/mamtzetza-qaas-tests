using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Google.Protobuf;
using OmegaFireflyTests.Assertions;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Generator;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Generators;

public record OmegaSoliderGeneratorConfig
{
    [Range(1, 1000)] public int Count { get; set; } = 5;
    public string IdPrefix { get; set; } = "SOL-";
    public string FavoriteFood { get; set; } = "Shawarma";
}

public class OmegaSoliderGenerator : BaseGenerator<OmegaSoliderGeneratorConfig>
{
    public override IEnumerable<Data<object>> Generate(IImmutableList<SessionData> sessions, IImmutableList<DataSource> sources)
    {
        if (Configuration.FavoriteFood != "Shawarma")
            throw new InvalidDataException("This reference suite uses reviewed Shawarma fixtures. Add explicit fixtures for other foods.");
        var fixtures = LiteralFixtures.Load();
        var runId = Guid.NewGuid().ToString("N")[..12];
        for (var i = 0; i < Configuration.Count; i++)
        {
            var soldier = fixtures[i % fixtures.Count].Input.Clone();
            soldier.SoldierId = $"{Configuration.IdPrefix}{runId}-{i + 1:D3}";
            yield return new Data<object> { Body = soldier.ToByteArray() };
        }
    }
}
