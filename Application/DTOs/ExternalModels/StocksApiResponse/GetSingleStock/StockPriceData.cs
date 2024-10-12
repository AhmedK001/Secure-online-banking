using System.Text.Json.Serialization;

namespace Application.DTOs.ExternalModels.StocksApiResponse.GetSingleStock;

public class StockPriceData
{
    [JsonPropertyName("1. open")]
    public string OpeningPrice { get; set; }

    [JsonPropertyName("2. high")]
    public string HighestPrice { get; set; }

    [JsonPropertyName("3. low")]
    public string LowestPrice { get; set; }

    [JsonPropertyName("4. close")]
    public string ClosingPrice { get; set; }

    [JsonPropertyName("5. volume")]
    public string TradingVolume { get; set; }
}