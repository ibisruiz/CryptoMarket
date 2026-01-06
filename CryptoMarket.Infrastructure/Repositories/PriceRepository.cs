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
    public class PriceRepository : IPriceRepository
    {
        private readonly AppDbContext context;
        public PriceRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Price>> GetLatestAsync(IEnumerable<string> symbols)
        {
            return await context.Prices
                .Where(p => symbols.Contains(p.Symbol))
                .OrderByDescending(p => p.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Price> prices)
        {
            await context.Prices.AddRangeAsync(prices);
            await context.SaveChangesAsync();
        }

        public async Task<List<Price>> GetLatestAllAsync()
        {
            return await context.Prices.AsNoTracking().ToListAsync();
        }

        public async Task ReplaceAllAsync(List<Price> prices)
        {
            context.Prices.RemoveRange(context.Prices);

            await context.Prices.AddRangeAsync(prices);

            await context.SaveChangesAsync();
        }
    }
}
