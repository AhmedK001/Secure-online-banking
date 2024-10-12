using System.Text.Json.Serialization;

namespace Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;

public class TopLosers
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; set; } = string.Empty;
    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;
    [JsonPropertyName("change_amount")]
    public string ChangeAmount { get; set; } = string.Empty;
    [JsonPropertyName("change_percentage")]
    public string ChangePercentage { get; set; } = string.Empty;
    [JsonPropertyName("volume")] public string Volume { get; set; } = string.Empty;
}