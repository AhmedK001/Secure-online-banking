using Adapters.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;

namespace Adapters.Interfaces;

public interface IExternalStockService
{
    Task<TopGainersResponse> GetTopGainers(GainersLosersActiveResponse gainersLosersActiveResponse);
    Task<TopLosersResponse> GetTopLosers(GainersLosersActiveResponse gainersLosersActiveResponse);
    Task<MostActivelyTradedResponse> GetMostActivelyStocks(GainersLosersActiveResponse gainersLosersActiveResponse);

}