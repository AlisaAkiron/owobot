using System.Text.Json;
using Microsoft.Extensions.Configuration;
using owoBot.Domain.Types;
using owoBot.Module.ExchangeRateApi.Models;
using owoBot.Module.ExchangeRateApi.Models.Internal;

namespace owoBot.Module.ExchangeRateApi.Services;

[AutoConstructor]
public partial class ExchangeRateApiClient
{
    private const string BaseUrl = "https://v6.exchangerate-api.com/v6/";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public async Task<ExchangeRateResponse> GetExchangeRate(CurrencyCode code)
    {
        var uri = Path.Join(BaseUrl, $"latest/{code}");

        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        var response = await SendRequestAsync<InternalExchangeRateResponse>(request);

        var exchangeRateResponse = new ExchangeRateResponse
        {
            LastUpdateTime = response.LastUpdateTime,
            NextUpdateTime = response.NextUpdateTime,
            BaseCode = CurrencyCode.From(response.BaseCode),
            ExchangeRates = response.Rates.ToDictionary(
                kvp => CurrencyCode.From(kvp.Key),
                kvp => kvp.Value
            )
        };

        return exchangeRateResponse;
    }

    private async Task<T> SendRequestAsync<T>(HttpRequestMessage request, CancellationToken? cancellationToken = null)
    {
        var response = await SendRequestAsync(request, cancellationToken);
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<T>(stream)
                     ?? throw new InvalidOperationException("Failed to deserialize response.");
        return result;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken? cancellationToken = null)
    {
        var ct = cancellationToken ?? CancellationToken.None;

        var httpClient = _httpClientFactory.CreateClient("Default");
        var token = _configuration["Modules:OpenExchangeRate:AppId"];
        request.Headers.Add("Authorization", $"Token {token}");
        var response = await httpClient.SendAsync(request, ct);

        if (response.IsSuccessStatusCode is false)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}. {content}");
        }

        return response;
    }
}
