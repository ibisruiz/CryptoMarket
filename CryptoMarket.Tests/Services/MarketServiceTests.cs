using CryptoMarket.Application.Interfaces;
using CryptoMarket.Application.Interfaces.Repositories;
using CryptoMarket.Domain.Entities;
using CryptoMarket.Infrastructure.Clients;
using CryptoMarket.Infrastructure.Repositories;
using CryptoMarket.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace CryptoMarket.Tests.Services
{
    public class MarketServiceTests
    {
        private readonly IMemoryCache _cache;
        private readonly Mock<ISymbolRepository> _symbolRepoMock;
        private readonly Mock<IPriceRepository> _priceRepoMock;
        private readonly Mock<ICandleRepository> _candleRepoMock;
        private readonly Mock<IBinanceClient> _binanceMock;

        private readonly MarketService _service;

        public MarketServiceTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());

            _symbolRepoMock = new Mock<ISymbolRepository>();
            _priceRepoMock = new Mock<IPriceRepository>();
            _candleRepoMock = new Mock<ICandleRepository>();
            _binanceMock = new Mock<IBinanceClient>();

            _service = new MarketService(_symbolRepoMock.Object, _priceRepoMock.Object, _candleRepoMock.Object, _cache, _binanceMock.Object);
        }

        [Fact]
        public async Task GetSymbolsAsync_returns_cache_when_exists()
        {
            var cachedSymbols = new List<Symbol>
        {
            new Symbol { Name = "BTCUSDT", BaseAsset = "BTC", QuoteAsset = "USDT" },
            new Symbol { Name = "ETHUSDT", BaseAsset = "ETH", QuoteAsset = "USDT" }
        };

            _cache.Set("symbols_cache", cachedSymbols);

            var result = await _service.GetSymbolsAsync();
                        
            Assert.Equal(2, result.Count);

            _symbolRepoMock.Verify(x => x.GetAllAsync(), Times.Never);
            _binanceMock.Verify(x => x.GetUsdtSymbolAsync(), Times.Never);
        }

        [Fact]
        public async Task GetPricesAsync_returns_prices_from_cache_when_snapshot_exists()
        {
            var symbols = new List<string> { "BTCUSDT", "ETHUSDT" };

            var cachedSnapshot = new List<Price>
            {
                new Price { Symbol = "BTCUSDT", Value = 50000 },
                new Price { Symbol = "ETHUSDT", Value = 3000 },
                new Price { Symbol = "BNBUSDT", Value = 400 }
            };

            _cache.Set("prices_cache", cachedSnapshot);

            var result = await _service.GetPricesAsync(symbols);
                        
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Symbol == "BTCUSDT");
            Assert.Contains(result, p => p.Symbol == "ETHUSDT");

            _priceRepoMock.Verify(x => x.GetLatestAllAsync(), Times.Never);
            _binanceMock.Verify(x => x.GetUpdatedPriceAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetPricesAsync_loads_from_db_when_cache_is_empty()
        {            
            var symbols = new List<string> { "BTCUSDT", "ETHUSDT" };

            var dbSnapshot = new List<Price>
            {
                new Price { Symbol = "BTCUSDT", Value = 50000 },
                new Price { Symbol = "ETHUSDT", Value = 3000 },
                new Price { Symbol = "BNBUSDT", Value = 400 }
            };

            _priceRepoMock
                .Setup(x => x.GetLatestAllAsync())
                .ReturnsAsync(dbSnapshot);

            var result = await _service.GetPricesAsync(symbols);

            Assert.Equal(2, result.Count);

            _priceRepoMock.Verify(x => x.GetLatestAllAsync(), Times.Once);
            _binanceMock.Verify(x => x.GetUpdatedPriceAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetPricesAsync_fetches_missing_symbols_from_binance()
        {
            var symbols = new List<string> { "BTCUSDT", "ETHUSDT", "BNBUSDT" };

            var dbSnapshot = new List<Price>
            {
                new Price { Symbol = "BTCUSDT", Value = 50000 }
            };

            _priceRepoMock
                .Setup(x => x.GetLatestAllAsync())
                .ReturnsAsync(dbSnapshot);

            _binanceMock
                .Setup(x => x.GetUpdatedPriceAsync("ETHUSDT"))
                .ReturnsAsync(new Price { Symbol = "ETHUSDT", Value = 3000 });

            _binanceMock
                .Setup(x => x.GetUpdatedPriceAsync("BNBUSDT"))
                .ReturnsAsync(new Price { Symbol = "BNBUSDT", Value = 400 });

            var result = await _service.GetPricesAsync(symbols);

            Assert.Equal(3, result.Count);

            _binanceMock.Verify(x => x.GetUpdatedPriceAsync("ETHUSDT"), Times.Once);
            _binanceMock.Verify(x => x.GetUpdatedPriceAsync("BNBUSDT"), Times.Once);

            _priceRepoMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Price>>()), Times.Once);
        }

        [Fact]
        public async Task GetCandlesAsync_returns_cache_when_exists()
        {
            string symbol = "BTCUSDT";
            string interval = "1m";
            int limit = 3;

            var cacheKey = $"candles:{symbol}:{interval}:{limit}";

            var cachedCandles = new List<Candle>
            {
                new Candle { Symbol = symbol, Open = 50000, Close = 50500 },
                new Candle { Symbol = symbol, Open = 50500, Close = 50200 },
                new Candle { Symbol = symbol, Open = 50200, Close = 50400 }
            };

            _cache.Set(cacheKey, cachedCandles);

            var result = await _service.GetCandlesAsync(symbol, interval, limit);

            Assert.Equal(3, result.Count);
            Assert.Equal(cachedCandles, result);

            _candleRepoMock.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _binanceMock.Verify(x => x.GetCandlesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCandlesAsync_loads_from_db_when_cache_empty()
        {
            string symbol = "BTCUSDT";
            string interval = "1m";
            int limit = 2;

            var cacheKey = $"candles:{symbol}:{interval}:{limit}";

            var dbCandles = new List<Candle>
            {
                new Candle { Symbol = symbol, Open = 50000, Close = 50500 },
                new Candle { Symbol = symbol, Open = 50500, Close = 50200 }
            };

            _candleRepoMock
                .Setup(x => x.GetAsync(symbol, interval, limit))
                .ReturnsAsync(dbCandles);

            var result = await _service.GetCandlesAsync(symbol, interval, limit);

            Assert.Equal(2, result.Count);
            Assert.Equal(dbCandles, result);

            Assert.True(_cache.TryGetValue(cacheKey, out List<Candle> cached));
            Assert.Equal(dbCandles, cached);

            _binanceMock.Verify(x => x.GetCandlesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCandlesAsync_fetches_from_binance_when_cache_and_db_empty()
        {
            string symbol = "BTCUSDT";
            string interval = "1m";
            int limit = 2;
            var cacheKey = $"candles:{symbol}:{interval}:{limit}";

            _candleRepoMock
                .Setup(x => x.GetAsync(symbol, interval, limit))
                .ReturnsAsync(new List<Candle>());

            var binanceCandles = new List<Candle>
            {
                new Candle { Symbol = symbol, Open = 50000, Close = 50500 },
                new Candle { Symbol = symbol, Open = 50500, Close = 50200 }
            };

            _binanceMock
                .Setup(x => x.GetCandlesAsync(symbol, interval, limit))
                .ReturnsAsync(binanceCandles);

            var result = await _service.GetCandlesAsync(symbol, interval, limit);

            Assert.Equal(2, result.Count);
            Assert.Equal(binanceCandles, result);

            _candleRepoMock.Verify(x => x.AddRangeAsync(binanceCandles), Times.Once);

            Assert.True(_cache.TryGetValue(cacheKey, out List<Candle> cached));
            Assert.Equal(binanceCandles, cached);
        }

        [Fact]
        public async Task RefreshSymbolsAsync_adds_only_new_symbols_and_invalidates_cache()
        {
            var binanceSymbols = new List<Symbol>
            {
                new Symbol { Name = "BTCUSDT", BaseAsset = "BTC", QuoteAsset = "USDT" },
                new Symbol { Name = "ETHUSDT", BaseAsset = "ETH", QuoteAsset = "USDT" },
                new Symbol { Name = "BNBUSDT", BaseAsset = "BNB", QuoteAsset = "USDT" }
            };

            var existingSymbols = new List<string> { "BTCUSDT", "ETHUSDT" };

            _binanceMock.Setup(x => x.GetUsdtSymbolAsync())
                .ReturnsAsync(binanceSymbols);

            _symbolRepoMock.Setup(x => x.GetExistingSymbolsAsync())
                .ReturnsAsync(existingSymbols);

            await _service.RefreshSymbolsAsync();

            _symbolRepoMock.Verify(x => x.AddRangeAsync(
                It.Is<List<Symbol>>(l => l.Count == 1 && l[0].Name == "BNBUSDT")
            ), Times.Once);

            Assert.False(_cache.TryGetValue("symbols_cache", out _));
            Assert.False(_cache.TryGetValue("prices_cache", out _));
            Assert.False(_cache.TryGetValue("candles_cache", out _));
        }

        [Fact]
        public async Task RefreshSymbolsAsync_does_nothing_if_no_new_symbols()
        {
            var binanceSymbols = new List<Symbol>
            {
                new Symbol { Name = "BTCUSDT", BaseAsset = "BTC", QuoteAsset = "USDT" }
            };

            var existingSymbols = new List<string> { "BTCUSDT" };

            _binanceMock.Setup(x => x.GetUsdtSymbolAsync())
                .ReturnsAsync(binanceSymbols);

            _symbolRepoMock.Setup(x => x.GetExistingSymbolsAsync())
                .ReturnsAsync(existingSymbols);

            await _service.RefreshSymbolsAsync();

            _symbolRepoMock.Verify(x => x.AddRangeAsync(It.IsAny<List<Symbol>>()), Times.Never);
        }

        [Fact]
        public async Task RefreshMarketAsync_invalidates_all_cache()
        {
            _cache.Set("symbols_cache", new List<Symbol>());
            _cache.Set("prices_cache", new List<object>());
            _cache.Set("candles_cache", new List<object>());

            await _service.RefreshMarketAsync();

            Assert.False(_cache.TryGetValue("symbols_cache", out _));
            Assert.False(_cache.TryGetValue("prices_cache", out _));
            Assert.False(_cache.TryGetValue("candles_cache", out _));
        }
    }
}
