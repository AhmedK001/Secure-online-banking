using System.Text.Json.Serialization;

namespace Application.DTOs.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;

public class GainersLosersActive
{
    [JsonPropertyName("last_updated")]
    private string LastUpdated { get; set; } = string.Empty;
    [JsonPropertyName("top_gainers")]
    public List<TopGainers>? TopGainers { get; set; }
    [JsonPropertyName("top_losers")]
    public List<TopLosers>? TopLosers { get; set; }
    [JsonPropertyName("most_actively_traded")]
    public List<MostActivelyTraded>? MostActivelyStocks { get; set; }
}