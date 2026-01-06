using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Domain.Entities
{
    public class Symbol
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string BaseAsset { get; set; } = null!; 
        public string QuoteAsset { get; set; } = null!;

    }
}
