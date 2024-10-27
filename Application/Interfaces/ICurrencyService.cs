using System.Text.Json.Nodes;
using Application.DTOs.ExternalModels.Currency;

namespace Application.Interfaces;

public interface ICurrencyService
{
    Task<(bool isFirstChance, JsonObject data)> GetExchangeRate(string currentCurrency, string aimedCurrency);
    Task<JsonObject> GetHistoricalExchangeRate(string currentCurrency, string aimedCurrency, string timeSeries);
}