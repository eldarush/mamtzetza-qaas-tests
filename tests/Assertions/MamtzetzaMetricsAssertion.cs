using System.Collections.Immutable;
using System.ComponentModel;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Assertion;
using QaaS.Framework.SDK.Session.SessionDataObjects;

namespace OmegaFireflyTests.Assertions;

public record MamtzetzaMetricsAssertionConfig
{
    [Description("Host where Mamtzetza exposes Prometheus metrics"), DefaultValue("127.0.0.1")]
    public string Host { get; set; } = "127.0.0.1";

    [Description("Port where Mamtzetza exposes Prometheus metrics"), DefaultValue(9090)]
    public int Port { get; set; } = 9090;

    [Description("Expected metrics path"), DefaultValue("/metrics")]
    public string Path { get; set; } = "/metrics";
}

public class MamtzetzaMetricsAssertion : BaseAssertion<MamtzetzaMetricsAssertionConfig>
{
    public override bool Assert(IImmutableList<SessionData> sessionDataList, IImmutableList<DataSource> dataSourceList)
    {
        var metricsUrl = $"http://{Configuration.Host}:{Configuration.Port}{Configuration.Path}";
        Context.Logger?.LogInformation("Asserting Prometheus metrics endpoint at {Url}...", metricsUrl);

        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = httpClient.GetAsync(metricsUrl).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                AssertionMessage = $"Metrics endpoint '{metricsUrl}' returned status code {(int)response.StatusCode} ({response.ReasonPhrase}).";
                AssertionTrace = "Expected HTTP 200 OK from Prometheus scrape endpoint.";
                return false;
            }

            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!content.Contains("mamtzetza_messages_received_total"))
            {
                AssertionMessage = "Metrics output does not contain expected metric 'mamtzetza_messages_received_total'.";
                AssertionTrace = content;
                return false;
            }

            if (!content.Contains("mamtzetza_messages_processed_total"))
            {
                AssertionMessage = "Metrics output does not contain expected metric 'mamtzetza_messages_processed_total'.";
                AssertionTrace = content;
                return false;
            }

            AssertionMessage = $"Prometheus metrics endpoint '{metricsUrl}' verified successfully with valid exposition format and counters.";
            AssertionTrace = $"Metrics response contained 'mamtzetza_messages_received_total' and 'mamtzetza_messages_processed_total'. Content length: {content.Length} bytes.";
            return true;
        }
        catch (Exception ex)
        {
            AssertionMessage = $"Failed to connect to Mamtzetza Prometheus metrics endpoint at '{metricsUrl}': {ex.Message}";
            AssertionTrace = ex.ToString();
            return false;
        }
    }
}
