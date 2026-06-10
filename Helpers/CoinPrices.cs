using Agent_Ollama.Loggers;
using Agent_Ollama.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Agent_Ollama.Helpers;

public static class CoinPrices
{
    private static readonly ILogger _logger = new ColorConsoleLogger("", () => new ColorConsoleLoggerConfiguration());

    [Description("Fetches the current price of a Bitcoin.")]
    public static async Task<string> GetBitcoinPrice(
    //[Description("Cryptocurrency symbol, e.g. 'BTC', 'ETH'")] string id,
    HttpClient httpClient)
    {
        try
        {
            // Example: replace with a real provider
            //var url = $"https://api.coinlore.net/api/ticker/?id={id.ToUpperInvariant()}";
            PriceResult? pr = null;

            var url = $"https://api.coinlore.net/api/ticker/?id=90";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var content = await httpClient.GetStringAsync(url);

            //var result = await httpClient.GetFromJsonAsync<PriceResult>(url);

            dynamic? obj = JsonSerializer.Deserialize<dynamic>(content);

            var list = JsonSerializer.Deserialize<List<JsonElement>>(content);
            foreach (var element in list!)
            {
                pr = JsonSerializer.Deserialize<PriceResult>(element);
                return pr?.price_usd ?? "0";
                //Console.WriteLine(element.GetProperty("price_us").GetString());
            }

            return "0";

            //    ? $"{id.ToUpperInvariant()}: ${result.price_us ?? "N/A"} USD"
            //    : $"Price for {id} not available.";
        }
        catch (Exception ex)
        {
            return $"Failed to retrieve price : {ex.Message}";
        }
    }


    [Description("Fetches the current prices of coins.")]
    public static async Task<List<PriceResult>> GetCoinPrices(HttpClient httpClient)
    {
        List<PriceResult> priceResults = new();
        
        //@"https://api.coinlore.net/api/tickers";

        Uri uri = new Uri(@"https://api.coinlore.net/api/tickers/?start=0&limit=20");
                
        try
        {
            var content = await httpClient.GetStringAsync(uri);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            //var list = await httpClient.GetFromJsonAsync<List<PriceResult>>(uri, options);


            JsonElement? root = JsonSerializer.Deserialize<dynamic>(content);
            JsonElement innerObject = root?.GetProperty("data") ?? default(JsonElement);

            // Deserialize directly to List<PriceResult>
            var list = JsonSerializer.Deserialize<List<PriceResult>>(innerObject, options);

            //dynamic? dinnerObject = JsonSerializer.Deserialize<dynamic>(innerObject);
            //var list = JsonSerializer.Deserialize<List<JsonElement>>(dinnerObject);

            //if (list is [var coinData, ..])
            //{
                foreach (var price in list!)
                {
                    //pr = JsonSerializer.Deserialize<PriceResult>(element);
                    priceResults.Add(price);

                    _logger.LogDebug($"{price!.symbol}", $"Coint {price!.name} has USD price of {price!.price_usd:C}");
                }

                // _logger.LogDebug($"Retrieved price data for {coinData.name}", $"Coin {coinData.name} has USD price of {coinData.price_usd:C}");
                // _logger.LogInformation($"Coin {coinData.name} has USD price of {coinData.price_usd:C}");
                //return coinData ?? new PriceResult();
            //}


            
            return priceResults;
        }
        catch (Exception ex)
        {
            //return $"Failed to retrieve price : {ex.Message}";
            Console.WriteLine(ex.Message);
            return new List<PriceResult>();
        }
    }


    [Description("Fetches the current price data of a coin.")]
    public static async Task<PriceResult> GetCoin(
    [Description("Cryptocurrency NUMERIC id for symbol, e.g. 90 = 'BTC', 80 = 'ETH'")] string id,
        HttpClient httpClient)
    {
        try
        {
            var url = $"https://api.coinlore.net/api/ticker/?id={id.ToUpperInvariant()}";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var results = await httpClient.GetFromJsonAsync<List<PriceResult>>(url, options);

            // Safely extract the first item using modern C# pattern matching
            if (results is [var coinData, ..])
            {
                _logger.LogDebug($"Retrieved price data for {coinData.name}", $"Coin {coinData.name} has USD price of {coinData.price_usd:C}");
                _logger.LogInformation($"Coin {coinData.name} has USD price of {coinData.price_usd:C}");
                return coinData ?? new PriceResult();
            }
    

            return new PriceResult();
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Network error invoking crypto provider: {ex.StatusCode}", ex);
        }
        catch (JsonException ex)
        {
            throw new JsonException("Failed to parse coin payload schema.", ex);
        }
        catch (Exception ex)
        {
            //logger.LogError(ex, "Unexpected error occurred while retrieving Bitcoin price from {Url}. \nError:: {Message}", url, ex.Message);
            throw new Exception("An unexpected error occurred.", ex);
        }
    }
}

