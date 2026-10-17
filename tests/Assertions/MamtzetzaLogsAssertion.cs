using System.Collections.Immutable;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record MamtzetzaLogsAssertionConfig
{
    public string ExpectedLoggerPrefix { get; set; } = "Mamtzetza";
    public string LogPath { get; set; } = "evidence/mamtzetza.jsonl";
    public string BaselinePath { get; set; } = "evidence/baseline.json";
}

public class MamtzetzaLogsAssertion : BaseAssertion<MamtzetzaLogsAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessions, IImmutableList<DataSource> sources)
    {
        try
        {
            var (inputs, outputs) = SessionMessages.Read(sessions);
            var baseline = EvidenceBaseline.Load(Configuration.BaselinePath);
            // Console logger is asynchronous; allow a bounded flush period.
            var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
            while (true)
            {
                try
                {
                    using var stream = new FileStream(Configuration.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    EvidenceValidation.ValidateLogs(reader.ReadToEnd(), outputs, Configuration.ExpectedLoggerPrefix,
                        baseline.StartedAtUnixMs, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    break;
                }
                catch (InvalidDataException) when (DateTimeOffset.UtcNow < deadline) { Thread.Sleep(100); }
            }
            AssertionMessage = $"Real stdout has publication evidence for all {inputs.Count} session IDs.";
            return true;
        }
        catch (Exception ex) { AssertionMessage = ex.Message; AssertionTrace = ex.ToString(); return false; }
    }
}
