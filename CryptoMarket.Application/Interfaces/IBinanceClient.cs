using CryptoMarket.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Application.Interfaces
{
    public interface IBinanceClient
    {
        Task<List<Symbol>> GetUsdtSymbolAsync();
        Task<Price> GetUpdatedPriceAsync(string symbol);
        Task<List<Candle>> GetCandlesAsync(string symbol, string interval, int limit);
        Task<List<Price>> GetAllUsdtPricesAsync();


    }
}
