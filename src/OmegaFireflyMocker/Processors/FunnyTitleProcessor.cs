using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using QaaS.Framework.SDK.DataSourceObjects;
using QaaS.Framework.SDK.Hooks.Processor;
using QaaS.Framework.SDK.Session.DataObjects;
using QaaS.Framework.SDK.Session.MetaDataObjects;

namespace OmegaFireflyMocker.Processors;

public record FunnyTitleProcessorConfig
{
    [Description("Default title to assign"), DefaultValue("Supreme Commander of Crispy Snacks")]
    public string DefaultTitle { get; set; } = "Supreme Commander of Crispy Snacks";

    [Description("Default comedic lore"), DefaultValue("Fights crime with crunch.")]
    public string DefaultLore { get; set; } = "Fights crime with crunch.";

    [Description("HTTP status code to return"), DefaultValue(200)]
    public int StatusCode { get; set; } = 200;
}

public class FunnyTitleProcessor : BaseTransactionProcessor<FunnyTitleProcessorConfig>
{
    public override Data<object> Process(IImmutableList<DataSource> dataSourceList, Data<object> requestData)
    {
        string title = Configuration.DefaultTitle;
        string lore = Configuration.DefaultLore;

        if (requestData.Body is byte[] bodyBytes && bodyBytes.Length > 0)
        {
            try
            {
                var bodyStr = Encoding.UTF8.GetString(bodyBytes);
                var jsonNode = JsonNode.Parse(bodyStr);
                var snack = jsonNode?["favoriteSnack"]?.ToString();
                if (!string.IsNullOrWhiteSpace(snack))
                {
                    title = $"Supreme Commander of {snack}";
                    lore = $"Wields the sacred energy of {snack} in the dark.";
                }
            }
            catch
            {
                // Fallback to default
            }
        }

        var responsePayload = new
        {
            title = title,
            funnyLore = lore
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
