using CryptoMarket.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : ControllerBase
    {
        private readonly IMarketService marketService;

        public MarketController(IMarketService marketService)
        {
            this.marketService = marketService;
        }

        [HttpGet("symbols")]
        public async Task<IActionResult> GetSymbols()
        {
            var symbols = await marketService.GetSymbolsAsync();
            return Ok(symbols);
        }

        [HttpPost("symbols/refresh")]

        public async Task<IActionResult> RefreshSymbols()
        {
            await marketService.RefreshSymbolsAsync();
            return NoContent();
        }

        [HttpGet("prices")]
        public async Task<IActionResult> GetPrices([FromQuery] string symbols)
        {
            if (string.IsNullOrWhiteSpace(symbols))
            {
                return BadRequest("Debe indicar al menos un simbolo");
            }

            var symbolList = symbols.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var prices = await marketService.GetPricesAsync(symbolList);

            return Ok(prices);
        }

        [HttpGet("candles/{symbol}")]
        public async Task<IActionResult> GetCandles(
        string symbol,
        [FromQuery] string interval = "1m",
        [FromQuery] int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Debe especificar un símbolo válido");
            }

            var candles = await marketService.GetCandlesAsync(symbol, interval, limit);

            return Ok(candles);
        }

        [HttpPost("market/refresh")]
        public async Task<IActionResult> RefreshMarket()
        {
            await marketService.RefreshMarketAsync();
            return NoContent();
        }
    }

}

