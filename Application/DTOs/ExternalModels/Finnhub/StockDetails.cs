using System.Text.Json.Serialization;

namespace Application.DTOs.ExternalModels.Finnhub;

public class StockDetails
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("displaySymbol")]
    public string DisplaySymbol { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}