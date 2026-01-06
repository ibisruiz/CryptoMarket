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
    public class SymbolRepository : ISymbolRepository
    {
        private readonly AppDbContext context;

        public SymbolRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Symbol>> GetAllAsync()
        {
            var items = await context.Symbols.AsNoTracking().ToListAsync();

            return items;
        }

        public async Task AddRangeAsync(IEnumerable<Symbol> symbols) 
        {
            await context.Symbols.AddRangeAsync(symbols);
            await context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync()
        {
            bool isAny = await context.Symbols.AnyAsync();
            return isAny;
        }

        public async Task<List<string>> GetExistingSymbolsAsync()
        {
            return await context.Symbols.Select(s => s.Name).ToListAsync();
        }
    }
}
