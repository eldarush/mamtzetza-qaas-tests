using System.Collections.Immutable;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record FireflyLogicalAssertionConfig
{
    public string ExpectedBuffName { get; set; } = "Quantum Disco Sparkles";
    public int ExpectedBonusGlow { get; set; } = 50;
    public bool ExpectExternalApiEnabled { get; set; } = true;
}

public class FireflyLogicalAssertion : BaseAssertion<FireflyLogicalAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessions, IImmutableList<DataSource> sources)
    {
        try
        {
            var (inputs, outputs) = SessionMessages.Read(sessions);
            var baseline = EvidenceBaseline.Load("evidence/baseline.json");
            EvidenceValidation.ValidateBatch(inputs, outputs, Configuration, baseline.StartedAtUnixMs);
            AssertionMessage = $"Validated {inputs.Count} distinct IDs, preserved and literal expected fields, exact buff/title, and timestamp.";
            return true;
        }
        catch (Exception ex) { AssertionMessage = ex.Message; AssertionTrace = ex.ToString(); return false; }
    }
}
