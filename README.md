# CryptoMarket

## Architecture
- Clean Architecture with layers:
  - **Domain**: Entities and business rules.
  - **Application**: Service and repository interfaces.
  - **Infrastructure**: Concrete implementations, EF Core, Binance client.
- Services are decoupled using **interfaces** for easy testing and mocking.
- Uses **IMemoryCache** with a **cache-aside** pattern.
- Persistence with **EF Core** and SQLite.

## Data Modeling
- **Symbol**: represents a USDT trading pair.
  - Name (PK): string, e.g., "BTCUSDT"
  - BaseAsset : string, e.g., "BTC"
  - QuoteAsset : string, e.g., "USDT"
- **Price**: represents the latest price of a symbol.
  - Id (PK)
  - Symbol: string
  - Value: decimal
  - Timestamp: DateTime
- **Candle**: represents historical OHLC data.
  - Id (PK)
  - Symbol: string
  - Interval: string, e.g., "1m"
  - Open, High, Low, Close: decimal
  - Volume: decimal
  - Timestamp: DateTime

## Main API Endpoints
- `GET /api/symbols` → retrieves all USDT symbols.
- `POST /api/symbols/refresh` → refreshes symbols from Binance.
- `GET /api/prices?symbols=BTCUSDT,ETHUSDT` → retrieves prices for multiple symbols.
- `GET /api/candles/{symbol}?interval=1m&limit=100` → retrieves OHLC candles.
- `POST /api/market/refresh` → invalidates the complete cache (symbols, prices, candles).

## Caching Strategy
- Symbols and prices stored in **IMemoryCache**.
- TTL for Prices: 60 seconds
- TTL for Candles: 5 minutes
- Cache is invalidated when refreshing symbols or market.
- A **complete snapshot of prices** is maintained to avoid incomplete results.

## Trade-offs
- SQLite is used for local development.
- Binance API calls are executed **asynchronously and in parallel** to avoid blocking.
- Full snapshot caching reduces external calls but increases memory usage.
- Historical prices are not persisted; only the latest value per symbol is stored.

## Testing
- Unit tests required using **xUnit**.
- All external Binance calls are **mocked**.
- Tests can be run from CLI: `dotnet test`.
- Test coverage includes:
  - `MarketService` (symbols, prices, candles)
  - Refresh methods
  - Cache behavior

## Future Improvements
- Migrating to a full database, eg., SQL Server, OracleDB
- Error handling and logging, with retries on Binance API calls.
