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

    [Description("Default favorite food to assign"), DefaultValue("Shawarma")]
    public string FavoriteFood { get; set; } = "Shawarma";
}

public class OmegaSoliderGenerator : BaseGenerator<OmegaSoliderGeneratorConfig>
{
    private static readonly string[] Names = ["John Vance", "Sarah Connor", "James Holden", "Ellen Ripley", "Arthur Dent", "Luke Skywalker", "Kara Thrace", "David Bowman"];
    private static readonly string[] Ranks = ["Captain", "General", "Sergeant", "Commander", "Admiral"];
    private static readonly int[] Ages = [22, 28, 36, 47, 58]; // Covers Rust, C#, Python, C++, LISP
    private static readonly string[] TvShows = ["Star Trek: TNG", "The Expanse", "The Matrix", "Doctor Who", "Cyberpunk: Edgerunners"];
    private static readonly string[] Planets = ["Mars", "Earth", "Jupiter", "Moon", "Venus"];
    private static readonly string[] Hobbies = ["Chess", "Astronomy", "Gaming", "Robotics", "Gardening"];
    private static readonly string[] Foods = ["Shawarma", "Spicy Ramen", "Quantum Pizza", "Falafel", "Space Tacos"];

    public override IEnumerable<Data<object>> Generate(
        IImmutableList<SessionData> sessionDataList,
        IImmutableList<DataSource> dataSourceList)
    {
        for (int i = 1; i <= Configuration.Count; i++)
        {
            int idx = (i - 1) % 5;
            var soldier = new OmegaSolider.Messages.OmegaSolider
            {
                SoldierId = $"{Configuration.IdPrefix}{i:D3}",
                Name = Names[(i - 1) % Names.Length],
                Rank = Ranks[idx],
                Age = Ages[idx],
                FavoriteFood = (i <= 5 && !string.IsNullOrWhiteSpace(Configuration.FavoriteFood)) ? Configuration.FavoriteFood : Foods[idx],
                FavoriteTvShow = TvShows[idx],
                ShoeSize = 42.0f + (idx * 0.5f),
                Height = 175.0f + (idx * 3.0f),
                Weight = 72.0f + (idx * 4.0f),
                LuckyNumber = 10 + idx, // 10 % 5 = 0 (Ada), 11 % 5 = 1 (Marie), 12 % 5 = 2 (Grace), etc.
                Hobby = Hobbies[idx],
                OriginPlanet = Planets[idx]
            };

            yield return new Data<object>
            {
                Body = soldier.ToByteArray()
            };
        }
    }
}
