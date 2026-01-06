using CryptoMarket.Application.Interfaces;
using CryptoMarket.Application.Interfaces.Repositories;
using CryptoMarket.Application.Interfaces.Services;
using CryptoMarket.Domain.Entities;
using CryptoMarket.Infrastructure.Clients;
using CryptoMarket.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Infrastructure.Services
{
    public class MarketService : IMarketService
    {
        private readonly ISymbolRepository symbolRepository;
        private readonly IPriceRepository priceRepository;
        private readonly ICandleRepository candleRepository;
        private readonly IMemoryCache memoryCache;
        private readonly IBinanceClient binanceClient;

        private const string SymbolsCacheKey = "symbols_cache";
        private const string PricesCacheKey = "prices_cache";
        private const string CandlesCacheKey = "candles_cache";


        public MarketService(ISymbolRepository symbolRepository, IPriceRepository priceRepository, ICandleRepository candleRepository,
                             IMemoryCache memoryCache, IBinanceClient binanceClient)
        {
            this.symbolRepository = symbolRepository;
            this.priceRepository = priceRepository;
            this.candleRepository = candleRepository;
            this.memoryCache = memoryCache;
            this.binanceClient = binanceClient;
        }

        public async Task<List<Symbol>> GetSymbolsAsync()
        {
            if (memoryCache.TryGetValue(SymbolsCacheKey, out List<Symbol> cachedSymbols))
            {
                return cachedSymbols;
            }

            bool hasSymbols = await symbolRepository.AnyAsync();            

            if (hasSymbols)
            {
                List<Symbol> dbSymbols = await symbolRepository.GetAllAsync();

                memoryCache.Set(SymbolsCacheKey, dbSymbols, TimeSpan.FromHours(1));

                return dbSymbols;
            }

            List<Symbol> binanceSymbols = await binanceClient.GetUsdtSymbolAsync();

            await symbolRepository.AddRangeAsync(binanceSymbols);

            memoryCache.Set(SymbolsCacheKey, binanceSymbols, TimeSpan.FromHours(1));

            return binanceSymbols;
        }

        public async Task RefreshSymbolsAsync()
        {
            List<Symbol> binanceSymbols = await binanceClient.GetUsdtSymbolAsync();

            if (binanceSymbols.Count == 0)
            {
                return;
            }

            List<string> existingSymbols = await symbolRepository.GetExistingSymbolsAsync();

            List<Symbol> newSymbols = binanceSymbols.Where(s => !existingSymbols.Contains(s.Name)).ToList();

            if (newSymbols.Count == 0)
            {
                return;
            }

            await symbolRepository.AddRangeAsync(newSymbols);

            memoryCache.Remove(SymbolsCacheKey);
            memoryCache.Remove(PricesCacheKey);
            memoryCache.Remove(CandlesCacheKey);
        }

        public async Task<List<Candle>> GetCandlesAsync(string symbol, string interval, int limit)
        {
            var candleItem = $"candles:{symbol}:{interval}:{limit}";

            if (memoryCache.TryGetValue(candleItem, out List<Candle> cachedCandles))
            {
                return cachedCandles;
            }

            var dbCandles = await candleRepository.GetAsync(symbol, interval, limit);

            if (dbCandles.Any())
            {
                memoryCache.Set(candleItem, dbCandles, TimeSpan.FromMinutes(5));
                return dbCandles;
            }

            var binanceCandles = await binanceClient.GetCandlesAsync(symbol, interval, limit);

            if (!binanceCandles.Any())
            {
                return new List<Candle>();
            }

            await candleRepository.AddRangeAsync(binanceCandles);

            memoryCache.Set(candleItem, binanceCandles, TimeSpan.FromMinutes(5));

            return binanceCandles;
        }

        public async Task RefreshMarketAsync()
        {
            memoryCache.Remove(SymbolsCacheKey);
            memoryCache.Remove(PricesCacheKey);
            memoryCache.Remove(CandlesCacheKey);

            await Task.CompletedTask;
        }

        public async Task<List<Price>> GetPricesAsync(List<string> symbols)
        {
            symbols = symbols.Distinct().ToList();  

            if (!memoryCache.TryGetValue(PricesCacheKey, out List<Price> snapshot))
            {
                snapshot = await priceRepository.GetLatestAllAsync();
                memoryCache.Set(PricesCacheKey, snapshot, TimeSpan.FromSeconds(60));
            }

            var snapshotSymbols = snapshot.Select(p => p.Symbol).ToHashSet();

            var missingSymbols = symbols
                .Where(s => !snapshotSymbols.Contains(s))
                .ToList();

            if (missingSymbols.Any())
            {
                var tasks = missingSymbols.Select(s => binanceClient.GetUpdatedPriceAsync(s));
                var fetchedPrices = (await Task.WhenAll(tasks)).ToList();

                await priceRepository.AddRangeAsync(fetchedPrices);

                snapshot.AddRange(fetchedPrices);
                memoryCache.Set(PricesCacheKey, snapshot, TimeSpan.FromSeconds(60));
            }

            return snapshot
                .Where(p => symbols.Contains(p.Symbol))
                .ToList();
        }

    }
}
 