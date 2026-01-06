using CryptoMarket.Application.Interfaces;
using CryptoMarket.Application.Interfaces.Repositories;
using CryptoMarket.Application.Interfaces.Services;
using CryptoMarket.Infrastructure.Clients;
using CryptoMarket.Infrastructure.Persistence;
using CryptoMarket.Infrastructure.Repositories;
using CryptoMarket.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=cryptomarket.db"));

builder.Services.AddScoped<ISymbolRepository, SymbolRepository>();
builder.Services.AddScoped<IPriceRepository, PriceRepository>();
builder.Services.AddScoped<ICandleRepository, CandleRepository>();
builder.Services.AddHttpClient<IBinanceClient, BinanceClient>( client =>
{
    client.BaseAddress = new Uri("https://api.binance.com");
});
builder.Services.AddScoped<IMarketService, MarketService>();
builder.Services.AddMemoryCache();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
