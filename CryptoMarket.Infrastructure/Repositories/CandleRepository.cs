using CryptoMarket.Application.Interfaces.Repositories;
using CryptoMarket.Domain.Entities;
using CryptoMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Infrastructure.Repositories
{
    public class CandleRepository : ICandleRepository
    {
        private readonly AppDbContext context;

        public CandleRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Candle>> GetAsync(string symbol, string interval, int limit)
        {
            return await context.Candles
                .Where(c => c.Symbol == symbol && c.Interval == interval)
                .OrderByDescending(c => c.OpenTime)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Candle> candles)
        {
            await context.Candles.AddRangeAsync(candles);
            await context.SaveChangesAsync();
        }

    }
}
