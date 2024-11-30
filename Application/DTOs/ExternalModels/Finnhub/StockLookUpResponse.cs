using System.Text.Json.Serialization;

namespace Application.DTOs.ExternalModels.Finnhub;

public class StockLookUpResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("result")]
    public List<StockDetails> Result { get; set; }
}