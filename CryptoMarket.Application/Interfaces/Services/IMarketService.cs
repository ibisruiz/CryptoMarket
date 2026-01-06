using CryptoMarket.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Application.Interfaces.Services
{
    public interface IMarketService
    {
        Task<List<Symbol>> GetSymbolsAsync();
        Task<List<Price>> GetPricesAsync(List<string> symbols);
        Task<List<Candle>> GetCandlesAsync(string symbol, string interval, int limit);
        Task RefreshSymbolsAsync();        
        Task RefreshMarketAsync();
    }
}
