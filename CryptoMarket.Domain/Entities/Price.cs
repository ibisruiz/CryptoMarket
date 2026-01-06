using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoMarket.Domain.Entities
{
    public class Price
    {
        public int Id { get; set; }

        public string Symbol { get; set; } = null!;
        public decimal Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
