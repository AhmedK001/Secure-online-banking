namespace Adapters.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;

public class TopLosersResponse
{
    public string Ticker { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string ChangeAmount { get; set; } = string.Empty;
    public string ChangePercentage { get; set; } = string.Empty;
    public string Volume { get; set; } = string.Empty;
}