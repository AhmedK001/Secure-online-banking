using System.Text.Json.Serialization;

namespace Application.DTOs.ExternalModels.Currency;

public class CurrencyExchangeRateResponse
{
    [JsonPropertyName("Realtime Currency Exchange Rate")]
    public ExchangeRateDto ExchangeRateDto { get; set; }
}