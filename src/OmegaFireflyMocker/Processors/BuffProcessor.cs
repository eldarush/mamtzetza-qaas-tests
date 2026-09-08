using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Processor;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;

namespace OmegaFireflyMocker.Processors;

public record BuffProcessorConfig
{
    [Description("Buff name to assign"), DefaultValue("Quantum Disco Sparkles")]
    public string DefaultBuffName { get; set; } = "Quantum Disco Sparkles";

    [Description("Bonus glow intensity points"), DefaultValue(50)]
    public int DefaultBonusGlow { get; set; } = 50;

    [Description("HTTP status code to return"), DefaultValue(200)]
    public int StatusCode { get; set; } = 200;
}

public class BuffProcessor : BaseTransactionProcessor<BuffProcessorConfig>
{
    public override Data<object> Process(IImmutableList<DataSource> dataSourceList, Data<object> requestData)
    {
        var soldierId = "UNKNOWN";

        // Try extracting soldierId from PathParameters
        if (requestData.MetaData?.Http?.PathParameters != null &&
            requestData.MetaData.Http.PathParameters.TryGetValue("soldierId", out var paramVal) &&
            !string.IsNullOrWhiteSpace(paramVal))
        {
            soldierId = paramVal;
        }
        else if (requestData.MetaData?.Http?.Uri != null)
        {
            var segments = requestData.MetaData.Http.Uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
                soldierId = segments[^1];
        }

        var responsePayload = new
        {
            soldierId = soldierId,
            buffName = Configuration.DefaultBuffName,
            bonusGlow = Configuration.DefaultBonusGlow
        };

        var json = JsonSerializer.Serialize(responsePayload);

        return new Data<object>
        {
            Body = Encoding.UTF8.GetBytes(json),
            MetaData = new MetaData
            {
                Http = new Http
                {
                    StatusCode = Configuration.StatusCode,
                    ResponseHeaders = new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/json"
                    }
                }
            }
        };
    }
}
