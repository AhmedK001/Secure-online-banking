namespace Application.DTOs.ExternalModels.StocksApiResponse.GetSingleStock;

public class StockPriceDetails
{
    public List<StockTimeSeries> TimeSeriesData { get; set; }
}