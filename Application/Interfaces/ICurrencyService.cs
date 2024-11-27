using System.Text.Json.Nodes;
using Application.DTOs.ExternalModels.Currency;
using Core.Enums;

namespace Application.Interfaces;

public interface ICurrencyService
{
    Task<(bool isFirstChance, JsonObject data)> GetExchangeRate(string currentCurrency, string aimedCurrency);
    Task<JsonObject> GetHistoricalExchangeRate(EnumCurrency currentCurrency, EnumCurrency aimedCurrency, string timeSeries);
    Task<ExchangeRateDto> GetExchangeForm(string fromCurrency, string toCurrency);
    Task<JsonObject?> FetchExchangeRate(string currentCurrency, string aimedCurrency);
    Task<ExchangeRateDto> GetOppositeExchangeRate(JsonObject exchangeRateResult);
}