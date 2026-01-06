using CryptoMarket.Application.Interfaces;
using CryptoMarket.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CryptoMarket.Infrastructure.Clients
{    
    public class BinanceClient : IBinanceClient
    {
        private readonly HttpClient httpClient;

        public BinanceClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<Symbol>> GetUsdtSymbolAsync()
        {
            var response = await httpClient.GetAsync("/api/v3/exchangeInfo");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            JsonDocument document = JsonDocument.Parse(json);

            List<Symbol> symbols = new List<Symbol>();

            foreach (var item in document.RootElement.GetProperty("symbols").EnumerateArray())
            {
                string quoteAssetJson = item.GetProperty("quoteAsset").GetString();

                if (quoteAssetJson == "USDT")
                {
                    Symbol symbol = new Symbol();
                    symbol.Name = item.GetProperty("symbol").GetString();
                    symbol.BaseAsset = item.GetProperty("baseAsset").GetString();
                    symbol.QuoteAsset = quoteAssetJson;

                    symbols.Add(symbol);
                }
            }

            return symbols;

        }

        public async Task<Price> GetUpdatedPriceAsync(string symbol)
        {
            var response = await httpClient.GetAsync($"/api/v3/ticker/price?symbol={symbol}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            JsonDocument document = JsonDocument.Parse(json);

            string jsonSymbol = document.RootElement.GetProperty("symbol").GetString();
            string priceString = document.RootElement.GetProperty("price").GetString();

            decimal priceValue = decimal.Parse(priceString, CultureInfo.InvariantCulture);

            Price price = new Price();
            price.Symbol = jsonSymbol;
            price.Value = priceValue;
            price.Timestamp = DateTime.UtcNow;

            return price;
        }

        public async Task<List<Candle>> GetCandlesAsync(string symbol, string interval, int limit)
        {
            var response = await httpClient.GetAsync($"/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            JsonDocument document = JsonDocument.Parse(json);

            List<Candle> candles = new List<Candle>();

            foreach (var item in document.RootElement.EnumerateArray())
            {
                Candle candle = new Candle();

                candle.Symbol = symbol;
                candle.Interval = interval;
                candle.OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(item[0].GetInt64()).UtcDateTime;
                candle.Open = decimal.Parse(item[1].GetString(), CultureInfo.InvariantCulture);
                candle.High = decimal.Parse(item[2].GetString(), CultureInfo.InvariantCulture);
                candle.Low = decimal.Parse(item[3].GetString(), CultureInfo.InvariantCulture);
                candle.Close = decimal.Parse(item[4].GetString(), CultureInfo.InvariantCulture);
                candle.Volume = decimal.Parse(item[5].GetString(), CultureInfo.InvariantCulture);

                candles.Add(candle);
            }

            return candles;
        }

        public async Task<List<Price>> GetAllUsdtPricesAsync()
        {
            var response = await httpClient.GetAsync("/api/v3/ticker/price");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(json);

            List<Price> prices = new();

            foreach (var item in document.RootElement.EnumerateArray())
            {
                var symbol = item.GetProperty("symbol").GetString();

                if (!symbol.EndsWith("USDT"))
                    continue;

                prices.Add(new Price
                {
                    Symbol = symbol,
                    Value = item.GetProperty("price").GetDecimal(),
                    Timestamp = DateTime.UtcNow
                });
            }

            return prices;
        }


    }
}
