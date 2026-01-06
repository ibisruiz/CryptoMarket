using CryptoMarket.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Application.Interfaces.Repositories
{
    public interface ISymbolRepository
    {
        Task<List<Symbol>> GetAllAsync();
        Task AddRangeAsync(IEnumerable<Symbol> symbols);
        Task<bool> AnyAsync();
        Task<List<string>> GetExistingSymbolsAsync();
    }
}
