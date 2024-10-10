namespace Adapters.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;

public class GainersLosersActiveResponse
{
    private string LastUpdated { get; set; } = string.Empty;
    public List<TopGainersResponse>? TopGainers { get; set; }
    public List<TopLosersResponse>? TopLosers { get; set; }
    public List<MostActivelyTradedResponse>? MostActivelyStocks { get; set; }
}