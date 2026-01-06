using CryptoMarket.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Application.Interfaces.Repositories
{
    public interface ICandleRepository
    {
        Task<List<Candle>> GetAsync(string symbol, string interval, int limit);
        Task AddRangeAsync(IEnumerable<Candle> candles);
    }
}
