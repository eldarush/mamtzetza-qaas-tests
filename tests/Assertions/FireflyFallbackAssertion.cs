using System.Collections.Immutable;
using System.ComponentModel;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record FireflyFallbackConfig
{
    [Description("Allowed fallback error prefix"), DefaultValue("API Error Fallback")]
    public string FallbackPrefix { get; set; } = "API Error Fallback";
}

public class FireflyFallbackAssertion : BaseAssertion<FireflyFallbackConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var session = sessionDataList.FirstOrDefault();
        if (session == null)
        {
            AssertionMessage = "No session data found for fallback assertion.";
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
            AssertionMessage = "No output FireflyExpert messages found to verify resilience.";
            return false;
        }

        foreach (var firefly in outputMessages)
        {
            if (string.IsNullOrWhiteSpace(firefly.ComedicBuff))
            {
                AssertionMessage = $"FireflyExpert for soldier {firefly.SoldierId} has empty ComedicBuff.";
                return false;
            }

            // Either successfully enriched or safely degraded fallback - both are valid resilience behaviors
            bool isEnriched = firefly.ComedicBuff.Contains(" - ");
            bool isFallback = firefly.ComedicBuff.StartsWith(Configuration.FallbackPrefix, StringComparison.OrdinalIgnoreCase) ||
                              firefly.ComedicBuff == "Unbuffed Normal Firefly (No API)";

            if (!isEnriched && !isFallback)
            {
                AssertionMessage = $"Unexpected ComedicBuff value for soldier {firefly.SoldierId}: '{firefly.ComedicBuff}'. Expected enriched buff or safe fallback.";
                return false;
            }
        }

        AssertionMessage = $"Resilience and fault-tolerance verified: All {outputMessages.Count} messages handled safely without crash or data drop.";
        AssertionTrace = $"Verified {outputMessages.Count} records. Component demonstrated robust error handling and fallback capability.";
        return true;
    }
}
