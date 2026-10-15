using System.Collections.Immutable;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record MamtzetzaMetricsAssertionConfig
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9090;
    public string Path { get; set; } = "/metrics";
    public string BaselinePath { get; set; } = "evidence/baseline.json";
    public bool ExpectExternalApiEnabled { get; set; } = true;
}

public class MamtzetzaMetricsAssertion : BaseAssertion<MamtzetzaMetricsAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessions, IImmutableList<DataSource> sources)
    {
        try
        {
            var (inputs, _) = SessionMessages.Read(sessions);
            var baseline = EvidenceBaseline.Load(Configuration.BaselinePath);
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var content = client.GetStringAsync($"http://{Configuration.Host}:{Configuration.Port}{Configuration.Path}").GetAwaiter().GetResult();
            EvidenceValidation.ValidateMetrics(baseline.Metrics, EvidenceValidation.ParseMetrics(content), inputs.Count, Configuration.ExpectExternalApiEnabled);
            AssertionMessage = $"Counter and histogram deltas match isolated batch of {inputs.Count}; zero processing/API errors.";
            return true;
        }
        catch (Exception ex) { AssertionMessage = ex.Message; AssertionTrace = ex.ToString(); return false; }
    }
}
