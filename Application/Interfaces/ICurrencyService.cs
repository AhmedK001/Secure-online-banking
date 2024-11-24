using System.Text.Json.Nodes;
using Application.DTOs.ExternalModels.Currency;
using Core.Enums;

namespace Application.Interfaces;

public interface ICurrencyService
{
    Task<(bool isFirstChance, JsonObject data)> GetExchangeRate(EnumCurrency currentCurrency, EnumCurrency aimedCurrency);
    Task<JsonObject> GetHistoricalExchangeRate(EnumCurrency currentCurrency, EnumCurrency aimedCurrency, string timeSeries);
    Task<ExchangeRateDto> GetExchangeForm(EnumCurrency fromCurrency, EnumCurrency toCurrency);
    Task<ExchangeRateDto> GetOppositeExchangeRate(JsonObject exchangeRateResult);
}