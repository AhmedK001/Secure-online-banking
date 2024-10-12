using System.Text.Json.Serialization;

namespace Application.DTOs.ExternalModels.StocksApiResponse.GetSingleStock;

public class StockTimeSeries
{
    public string Timestamp { get; set; }
    public StockPriceData PriceData { get; set; }
}