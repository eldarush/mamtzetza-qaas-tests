using System.Collections.Immutable;
using System.ComponentModel;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record FireflyLogicalAssertionConfig
{
    [Description("Expected external API buff name if feature flag is active"), DefaultValue("Quantum Disco Sparkles")]
    public string ExpectedBuffName { get; set; } = "Quantum Disco Sparkles";

    [Description("Expected bonus glow if feature flag is active"), DefaultValue(50)]
    public int ExpectedBonusGlow { get; set; } = 50;

    [Description("Whether external API feature is expected to be enabled on the target component"), DefaultValue(true)]
    public bool ExpectExternalApiEnabled { get; set; } = true;
}

public class FireflyLogicalAssertion : BaseAssertion<FireflyLogicalAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var session = sessionDataList.FirstOrDefault();
        if (session == null)
        {
            AssertionMessage = "No session data found to assert against.";
            AssertionTrace = "sessionDataList is empty.";
            return false;
        }

        var inputMessages = new List<OmegaSolider.Messages.OmegaSolider>();
        foreach (var input in session.Inputs ?? [])
        {
            foreach (var data in input.Data)
            {
                if (data.Body is OmegaSolider.Messages.OmegaSolider soliderObj)
                {
                    inputMessages.Add(soliderObj);
                }
                else if (data.Body is byte[] bodyBytes)
                {
                    try
                    {
                        var parsed = OmegaSolider.Messages.OmegaSolider.Parser.ParseFrom(bodyBytes);
                        inputMessages.Add(parsed);
                    }
                    catch (Exception ex)
                    {
                        Context.Logger?.LogWarning("Failed to parse input body as OmegaSolider: {Message}", ex.Message);
                    }
                }
            }
        }

        var outputMessages = new List<OmegaSolider.Messages.FireflyExpert>();
        foreach (var output in session.Outputs ?? [])
        {
            foreach (var data in output.Data)
            {
                if (data.Body is OmegaSolider.Messages.FireflyExpert expertObj)
                {
                    outputMessages.Add(expertObj);
                }
                else if (data.Body is byte[] bodyBytes)
                {
                    try
                    {
                        var parsed = OmegaSolider.Messages.FireflyExpert.Parser.ParseFrom(bodyBytes);
                        outputMessages.Add(parsed);
                    }
                    catch (Exception ex)
                    {
                        Context.Logger?.LogWarning("Failed to parse output body as FireflyExpert: {Message}", ex.Message);
                    }
                }
            }
        }

        if (inputMessages.Count == 0)
        {
            AssertionMessage = "No input OmegaSolider messages found in session inputs.";
            AssertionTrace = $"Input message count was 0. Session Inputs count: {session.Inputs?.Count ?? 0}.";
            return false;
        }

        if (outputMessages.Count == 0)
        {
            AssertionMessage = "No output FireflyExpert messages found in session outputs.";
            AssertionTrace = $"Output message count was 0. Session Outputs count: {session.Outputs?.Count ?? 0}.";
            return false;
        }

        var traces = new List<string>();

        foreach (var input in inputMessages)
        {
            var matchingOutput = outputMessages.FirstOrDefault(o => o.SoldierId == input.SoldierId);
            if (matchingOutput == null)
            {
                AssertionMessage = $"Verification failed: Missing output FireflyExpert for SoldierId '{input.SoldierId}'.";
                AssertionTrace = $"Available output IDs: [{string.Join(", ", outputMessages.Select(o => o.SoldierId))}].";
                return false;
            }

            // 1. Check preserved identity & physical attributes
            if (matchingOutput.Name != input.Name)
            {
                AssertionMessage = $"Name mismatch for soldier {input.SoldierId}: expected '{input.Name}', got '{matchingOutput.Name}'.";
                return false;
            }

            if (matchingOutput.Rank != input.Rank)
            {
                AssertionMessage = $"Rank mismatch for soldier {input.SoldierId}: expected '{input.Rank}', got '{matchingOutput.Rank}'.";
                return false;
            }

            if (matchingOutput.Age != input.Age)
            {
                AssertionMessage = $"Age mismatch for soldier {input.SoldierId}: expected '{input.Age}', got '{matchingOutput.Age}'.";
                return false;
            }

            if (matchingOutput.FavoriteFood != input.FavoriteFood)
            {
                AssertionMessage = $"FavoriteFood mismatch for soldier {input.SoldierId}: expected '{input.FavoriteFood}', got '{matchingOutput.FavoriteFood}'.";
                return false;
            }

            if (matchingOutput.FavoriteTvShow != input.FavoriteTvShow)
            {
                AssertionMessage = $"FavoriteTvShow mismatch for soldier {input.SoldierId}: expected '{input.FavoriteTvShow}', got '{matchingOutput.FavoriteTvShow}'.";
                return false;
            }

            if (Math.Abs(matchingOutput.ShoeSize - input.ShoeSize) > 0.01f)
            {
                AssertionMessage = $"ShoeSize mismatch for soldier {input.SoldierId}: expected '{input.ShoeSize}', got '{matchingOutput.ShoeSize}'.";
                return false;
            }

            if (Math.Abs(matchingOutput.Height - input.Height) > 0.01f)
            {
                AssertionMessage = $"Height mismatch for soldier {input.SoldierId}: expected '{input.Height}', got '{matchingOutput.Height}'.";
                return false;
            }

            if (Math.Abs(matchingOutput.Weight - input.Weight) > 0.01f)
            {
                AssertionMessage = $"Weight mismatch for soldier {input.SoldierId}: expected '{input.Weight}', got '{matchingOutput.Weight}'.";
                return false;
            }

            if (matchingOutput.LuckyNumber != input.LuckyNumber)
            {
                AssertionMessage = $"LuckyNumber mismatch for soldier {input.SoldierId}: expected '{input.LuckyNumber}', got '{matchingOutput.LuckyNumber}'.";
                return false;
            }

            if (matchingOutput.Hobby != input.Hobby)
            {
                AssertionMessage = $"Hobby mismatch for soldier {input.SoldierId}: expected '{input.Hobby}', got '{matchingOutput.Hobby}'.";
                return false;
            }

            if (matchingOutput.OriginPlanet != input.OriginPlanet)
            {
                AssertionMessage = $"OriginPlanet mismatch for soldier {input.SoldierId}: expected '{input.OriginPlanet}', got '{matchingOutput.OriginPlanet}'.";
                return false;
            }

            // 2. Check 5 Deterministically Calculated Fields
            string expectedTech = CalculateExpectedTechnology(input.FavoriteTvShow, input.Hobby);
            if (matchingOutput.FavoriteTechnology != expectedTech)
            {
                AssertionMessage = $"FavoriteTechnology mismatch for soldier {input.SoldierId}: expected '{expectedTech}', got '{matchingOutput.FavoriteTechnology}'.";
                return false;
            }

            string expectedTeam = CalculateExpectedTeam(input.OriginPlanet);
            if (matchingOutput.FavoriteTeam != expectedTeam)
            {
                AssertionMessage = $"FavoriteTeam mismatch for soldier {input.SoldierId}: expected '{expectedTeam}', got '{matchingOutput.FavoriteTeam}'.";
                return false;
            }

            string expectedCommander = CalculateExpectedCommander(input.Rank);
            if (matchingOutput.FavoriteCommander != expectedCommander)
            {
                AssertionMessage = $"FavoriteCommander mismatch for soldier {input.SoldierId}: expected '{expectedCommander}', got '{matchingOutput.FavoriteCommander}'.";
                return false;
            }

            string expectedWoman = CalculateExpectedWoman(input.LuckyNumber);
            if (matchingOutput.FavoriteWoman != expectedWoman)
            {
                AssertionMessage = $"FavoriteWoman mismatch for soldier {input.SoldierId}: expected '{expectedWoman}', got '{matchingOutput.FavoriteWoman}'.";
                return false;
            }

            string expectedCodingLang = CalculateExpectedCodingLanguage(input.Age);
            if (matchingOutput.FavoriteCodingLanguage != expectedCodingLang)
            {
                AssertionMessage = $"FavoriteCodingLanguage mismatch for soldier {input.SoldierId}: expected '{expectedCodingLang}', got '{matchingOutput.FavoriteCodingLanguage}'.";
                return false;
            }

            // 3. Check Glow Calculation
            int expectedBaseGlow = (int)(input.Height + (input.Weight * 0.5f)) + (Math.Abs(input.LuckyNumber) % 10);
            int expectedTotalGlow = Configuration.ExpectExternalApiEnabled
                ? expectedBaseGlow + Configuration.ExpectedBonusGlow
                : expectedBaseGlow;

            if (matchingOutput.GlowIntensity != expectedTotalGlow)
            {
                AssertionMessage = $"GlowIntensity mismatch for soldier {input.SoldierId}: expected '{expectedTotalGlow}' (Base: {expectedBaseGlow}, Bonus: {(Configuration.ExpectExternalApiEnabled ? Configuration.ExpectedBonusGlow : 0)}), got '{matchingOutput.GlowIntensity}'.";
                return false;
            }

            // 4. Check Comedic Buff
            if (Configuration.ExpectExternalApiEnabled)
            {
                if (string.IsNullOrWhiteSpace(Configuration.ExpectedBuffName) ||
                    !matchingOutput.ComedicBuff.Contains(Configuration.ExpectedBuffName, StringComparison.OrdinalIgnoreCase))
                {
                    AssertionMessage = $"ComedicBuff mismatch for soldier {input.SoldierId}: expected to contain '{Configuration.ExpectedBuffName}', got '{matchingOutput.ComedicBuff}'.";
                    return false;
                }
            }
            else
            {
                if (matchingOutput.ComedicBuff != "Unbuffed Normal Firefly (No API)")
                {
                    AssertionMessage = $"ComedicBuff mismatch for soldier {input.SoldierId}: expected 'Unbuffed Normal Firefly (No API)', got '{matchingOutput.ComedicBuff}'.";
                    return false;
                }
            }

            traces.Add($"Verified {input.SoldierId}: Name='{matchingOutput.Name}', Tech='{matchingOutput.FavoriteTechnology}', Team='{matchingOutput.FavoriteTeam}', Commander='{matchingOutput.FavoriteCommander}', Woman='{matchingOutput.FavoriteWoman}', Lang='{matchingOutput.FavoriteCodingLanguage}', Glow={matchingOutput.GlowIntensity}");
        }

        AssertionMessage = $"All {inputMessages.Count} OmegaSolider messages successfully validated against logical transformation rules, calculations, and enrichment.";
        AssertionTrace = string.Join("; ", traces);
        return true;
    }

    private static string CalculateExpectedTechnology(string tvShow, string hobby)
    {
        var show = tvShow ?? string.Empty;
        if (show.Contains("Trek", StringComparison.OrdinalIgnoreCase) || show.Contains("Star Wars", StringComparison.OrdinalIgnoreCase))
            return "Antimatter Warp Core";
        if (show.Contains("Expanse", StringComparison.OrdinalIgnoreCase))
            return "Epstein Fusion Drive";
        if (show.Contains("Matrix", StringComparison.OrdinalIgnoreCase))
            return "Neural Direct Link";
        if (show.Contains("Doctor Who", StringComparison.OrdinalIgnoreCase) || show.Contains("TARDIS", StringComparison.OrdinalIgnoreCase))
            return "TARDIS Chrono-Engine";
        if (show.Contains("Cyberpunk", StringComparison.OrdinalIgnoreCase))
            return "Sandevistan Neural Implant";
        if (show.Contains("Firefly", StringComparison.OrdinalIgnoreCase))
            return "Serenity Gravity Rotor";

        return !string.IsNullOrWhiteSpace(hobby) ? $"Quantum {hobby} Disruptor" : "Quantum Tachyon Disruptor";
    }

    private static string CalculateExpectedTeam(string originPlanet)
    {
        return (originPlanet?.Trim()?.ToLowerInvariant()) switch
        {
            "mars" => "Martian Dust Devils",
            "earth" => "Terran Cyber Knights",
            "jupiter" => "Great Red Spot Cyclones",
            "moon" => "Lunar Eclipse Titans",
            "venus" => "Venusian Storm Chasers",
            "saturn" => "Saturnian Ring Walkers",
            _ => $"{originPlanet} Galactic Starfighters"
        };
    }

    private static string CalculateExpectedCommander(string rank)
    {
        var r = rank ?? string.Empty;
        if (r.Contains("General", StringComparison.OrdinalIgnoreCase) || r.Contains("Commander", StringComparison.OrdinalIgnoreCase))
            return "General Kenobi";
        if (r.Contains("Captain", StringComparison.OrdinalIgnoreCase))
            return "Captain Jean-Luc Picard";
        if (r.Contains("Sergeant", StringComparison.OrdinalIgnoreCase) || r.Contains("Major", StringComparison.OrdinalIgnoreCase))
            return "Sergeant Avery Johnson";
        if (r.Contains("Admiral", StringComparison.OrdinalIgnoreCase))
            return "Admiral William Adama";
        if (r.Contains("Colonel", StringComparison.OrdinalIgnoreCase))
            return "Colonel Jack O'Neill";

        return "Commander Shepard";
    }

    private static string CalculateExpectedWoman(int luckyNumber)
    {
        return (Math.Abs(luckyNumber) % 5) switch
        {
            0 => "Ada Lovelace",
            1 => "Marie Curie",
            2 => "Grace Hopper",
            3 => "Margaret Hamilton",
            4 => "Hedy Lamarr",
            _ => "Ada Lovelace"
        };
    }

    private static string CalculateExpectedCodingLanguage(int age)
    {
        if (age < 25) return "Rust";
        if (age < 35) return "C#";
        if (age < 45) return "Python";
        if (age < 55) return "C++";
        return "LISP";
    }
}
