using Adapters.ExternalModels.StocksApiResponse.GetTopGainers_Losers_Actice;
using Adapters.Interfaces;

namespace Adapters.Services;

public class ExternalStockService : IExternalStockService
{
    public Task<TopGainersResponse> GetTopGainers(GainersLosersActiveResponse gainersLosersActiveResponse)
    {
        throw new NotImplementedException();
    }

    public Task<TopLosersResponse> GetTopLosers(GainersLosersActiveResponse gainersLosersActiveResponse)
    {
        throw new NotImplementedException();
    }

    public Task<MostActivelyTradedResponse> GetMostActivelyStocks(GainersLosersActiveResponse gainersLosersActiveResponse)
    {
        throw new NotImplementedException();
    }
}