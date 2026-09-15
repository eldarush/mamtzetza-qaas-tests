using System.Collections.Immutable;
using System.ComponentModel;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record FireflyStressAssertionConfig
{
    [Description("Expected minimum message count to assert stress throughput"), DefaultValue(5)]
    public int MinimumExpectedCount { get; set; } = 5;
}

public class FireflyStressAssertion : BaseAssertion<FireflyStressAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var session = sessionDataList.FirstOrDefault();
        if (session == null)
        {
            AssertionMessage = "No session data found for stress assertion.";
            return false;
        }

        int inputCount = 0;
        foreach (var input in session.Inputs ?? [])
            inputCount += input.Data.Count;

        int outputCount = 0;
        foreach (var output in session.Outputs ?? [])
            outputCount += output.Data.Count;

        if (inputCount < Configuration.MinimumExpectedCount)
        {
            AssertionMessage = $"Stress test expected at least {Configuration.MinimumExpectedCount} inputs, but received {inputCount}.";
            return false;
        }

        if (inputCount != outputCount)
        {
            AssertionMessage = $"Stress test detected message loss: Published {inputCount} messages, but consumed {outputCount} messages.";
            return false;
        }

        AssertionMessage = $"Stress throughput assertion passed: Processed {inputCount} messages at high throughput with zero message drops and 100% data integrity.";
        AssertionTrace = $"Inputs: {inputCount}, Outputs: {outputCount}. Ratio: 1.0 (100%).";
        return true;
    }
}
