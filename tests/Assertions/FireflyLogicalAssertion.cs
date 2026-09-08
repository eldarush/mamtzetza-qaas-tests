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

            // Check preserved fields
            if (matchingOutput.Codename != input.Codename)
            {
                AssertionMessage = $"Codename mismatch for soldier {input.SoldierId}: expected '{input.Codename}', got '{matchingOutput.Codename}'.";
                return false;
            }

            if (matchingOutput.RankLevel != input.RankLevel)
            {
                AssertionMessage = $"RankLevel mismatch for soldier {input.SoldierId}: expected '{input.RankLevel}', got '{matchingOutput.RankLevel}'.";
                return false;
            }

            // Check calculation logic
            int expectedBaseGlow = (input.RankLevel * 10) + (input.BraveryPoints * 2);
            int expectedTotalGlow = Configuration.ExpectExternalApiEnabled
                ? expectedBaseGlow + Configuration.ExpectedBonusGlow
                : expectedBaseGlow;

            if (matchingOutput.GlowIntensity != expectedTotalGlow)
            {
                AssertionMessage = $"GlowIntensity mismatch for soldier {input.SoldierId}: expected '{expectedTotalGlow}' (Base: {expectedBaseGlow}, Bonus: {(Configuration.ExpectExternalApiEnabled ? Configuration.ExpectedBonusGlow : 0)}), got '{matchingOutput.GlowIntensity}'.";
                return false;
            }

            // Check comedic buff
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

            // Check expertise string
            string expectedExpertise = $"Expert in {input.FavoriteSnack} Logistics";
            if (matchingOutput.Expertise != expectedExpertise)
            {
                AssertionMessage = $"Expertise mismatch for soldier {input.SoldierId}: expected '{expectedExpertise}', got '{matchingOutput.Expertise}'.";
                return false;
            }

            traces.Add($"Verified {input.SoldierId}: Codename='{matchingOutput.Codename}', Glow={matchingOutput.GlowIntensity}, Buff='{matchingOutput.ComedicBuff}'");
        }

        AssertionMessage = $"All {inputMessages.Count} OmegaSolider messages successfully transformed into valid FireflyExpert records according to business logic.";
        AssertionTrace = string.Join("; ", traces);
        return true;
    }
}
