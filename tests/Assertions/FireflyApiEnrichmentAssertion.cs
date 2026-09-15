using System.Collections.Immutable;
using System.ComponentModel;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record FireflyApiEnrichmentConfig
{
    [Description("Expected buff name from external API"), DefaultValue("Quantum Disco Sparkles")]
    public string ExpectedBuffName { get; set; } = "Quantum Disco Sparkles";

    [Description("Expected bonus glow from external API"), DefaultValue(50)]
    public int ExpectedBonusGlow { get; set; } = 50;
}

public class FireflyApiEnrichmentAssertion : BaseAssertion<FireflyApiEnrichmentConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var session = sessionDataList.FirstOrDefault();
        if (session == null)
        {
            AssertionMessage = "No session data found for API enrichment assertion.";
            return false;
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
                    catch { }
                }
            }
        }

        if (outputMessages.Count == 0)
        {
            AssertionMessage = "No output FireflyExpert messages found to verify external API enrichment.";
            return false;
        }

        foreach (var firefly in outputMessages)
        {
            if (!firefly.ComedicBuff.Contains(Configuration.ExpectedBuffName, StringComparison.OrdinalIgnoreCase))
            {
                AssertionMessage = $"Soldier {firefly.SoldierId} does not contain expected buff '{Configuration.ExpectedBuffName}'. ComedicBuff was: '{firefly.ComedicBuff}'.";
                return false;
            }

            if (!firefly.ComedicBuff.Contains(" - "))
            {
                AssertionMessage = $"Soldier {firefly.SoldierId} ComedicBuff does not contain title separator ' - ': '{firefly.ComedicBuff}'.";
                return false;
            }
        }

        AssertionMessage = $"External API enrichment verified: All {outputMessages.Count} FireflyExpert records were successfully enriched via HTTP Mocker.";
        AssertionTrace = $"Buff '{Configuration.ExpectedBuffName}' and comedic titles confirmed across all {outputMessages.Count} messages.";
        return true;
    }
}
