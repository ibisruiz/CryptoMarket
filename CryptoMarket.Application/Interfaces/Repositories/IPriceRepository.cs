using CryptoMarket.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Application.Interfaces.Repositories
{
    public interface IPriceRepository
    {
        Task<List<Price>> GetLatestAsync(IEnumerable<string> symbols);
        Task AddRangeAsync(IEnumerable<Price> prices);
        Task ReplaceAllAsync(List<Price> prices);
        Task<List<Price>> GetLatestAllAsync(); 
    }
}
