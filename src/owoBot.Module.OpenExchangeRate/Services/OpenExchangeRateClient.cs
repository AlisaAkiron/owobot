using System.Text.Json;
using Microsoft.Extensions.Configuration;
using owoBot.Module.OpenExchangeRate.Extensions;
using owoBot.Module.OpenExchangeRate.Models;
using owoBot.Module.OpenExchangeRate.Models.Internal;

namespace owoBot.Module.OpenExchangeRate.Services;

[AutoConstructor]
public partial class OpenExchangeRateClient
{
    private const string BaseUrl = "https://openexchangerates.org/api/";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public async Task<List<Currency>> GetCurrencies(bool showAlternative = false, bool showInactive = false, CancellationToken? cancellationToken = null)
    {
        var uri = Path.Join(BaseUrl, "currencies.json")
            .AddQuery("prettyprint", "false")
            .AddQuery("show_alternative", showAlternative)
            .AddQuery("show_inactive", showInactive);

        var req = new HttpRequestMessage(HttpMethod.Get, uri);

        var resp = await SendRequestAsync<Dictionary<string, string>>(req, cancellationToken);

        var currencies = resp.Select(x => new Currency
        {
            Code = x.Key,
            Name = x.Value
        })
        .ToList();

        return currencies;
    }

    public async Task<CurrencyExchangeRate> GetCurrencyExchangeRate(string baseCurrency = "USD", List<string>? symbols = null, bool showAlternative = false, CancellationToken? cancellationToken = null)
    {
        var uri = Path.Join(BaseUrl, "latest.json")
            .AddQuery("prettyprint", "false")
            .AddQuery("base", baseCurrency)
            .AddQuery("show_alternative", showAlternative);
        if (symbols?.Count > 0)
        {
            uri = uri.AddQuery("symbols", string.Join(",", symbols));
        }

        var req = new HttpRequestMessage(HttpMethod.Get, uri);

        var resp = await SendRequestAsync<InternalExchangeRate>(req, cancellationToken);

        return new CurrencyExchangeRate
        {
            Base = resp.Base,
            Timestamp = resp.Timestamp,
            Rates = resp.Rates
        };
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
        var token = _configuration["Modules:OpenExchangeRate:Token"];
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
