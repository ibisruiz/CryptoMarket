using Microsoft.EntityFrameworkCore;
using CryptoMarket.Domain.Entities;

namespace CryptoMarket.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Symbol> Symbols => Set<Symbol>();
        public DbSet<Price> Prices => Set<Price>();
        public DbSet<Candle> Candles => Set<Candle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Symbol>(entity =>
            {
                entity.HasIndex(x => x.Name).IsUnique();
                entity.Property(x => x.Name).IsRequired();
            });

            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasIndex(x => new { x.Symbol, x.Timestamp });
            });

            modelBuilder.Entity<Candle>(entity =>
            {
                entity.HasIndex(x => new { x.Symbol, x.Interval, x.OpenTime });
            });
        }

    }
}
