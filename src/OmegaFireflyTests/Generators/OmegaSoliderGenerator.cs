using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Google.Protobuf;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Generator;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Generators;

public record OmegaSoliderGeneratorConfig
{
    [Description("Number of OmegaSolider messages to generate"), Range(1, 1000), DefaultValue(5)]
    public int Count { get; set; } = 5;

    [Description("Prefix for soldier ID"), DefaultValue("SOL-")]
    public string IdPrefix { get; set; } = "SOL-";

    [Description("Default favorite snack to assign"), DefaultValue("Quantum Doritos")]
    public string FavoriteSnack { get; set; } = "Quantum Doritos";
}

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
