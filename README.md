# Share Data Fetcher

A TypeScript library for fetching financial share data from multiple providers including Alpha Vantage, Yahoo Finance, and IEX Cloud.

## Features

- Fetch real-time share prices
- Fetch historical share data (daily, weekly, monthly)
- Support for multiple financial data providers
- Built-in caching to reduce API calls
- TypeScript type safety
- Comprehensive error handling
- Easy to use API

## Installation

```bash
npm install share-data-fetcher
```

## Usage

### Basic Usage

```typescript
import { ShareDataFetcher, DataProvider } from 'share-data-fetcher';

// Initialize the fetcher
const fetcher = new ShareDataFetcher({
  provider: DataProvider.ALPHA_VANTAGE,
  apiKey: 'YOUR_API_KEY',
  baseUrl: 'https://www.alphavantage.co'
});

// Fetch real-time data
const realTimeData = await fetcher.fetchRealTimeData('AAPL');
console.log(realTimeData);

// Fetch historical data
const historicalData = await fetcher.fetchHistoricalData({
  symbol: 'AAPL',
  frequency: DataFrequency.DAILY
});
console.log(historicalData);
```

### Configuration

The fetcher requires an API configuration object:

```typescript
interface ApiConfig {
  provider: DataProvider;  // ALPHA_VANTAGE, YAHOO_FINANCE, or IEX_CLOUD
  apiKey: string;          // Your API key for the selected provider
  baseUrl: string;         // Base URL for the API
}
```

### Data Models

#### RealTimeShareData

```typescript
interface RealTimeShareData {
  symbol: string;
  price: number;
  change: number;
  changePercent: number;
  volume: number;
  timestamp: Date;
}
```

#### HistoricalShareData

```typescript
interface HistoricalShareData {
  date: Date;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
}
```

## API Reference

### ShareDataFetcher

#### Constructor

```typescript
new ShareDataFetcher(config: ApiConfig)
```

#### fetchRealTimeData

```typescript
fetchRealTimeData(symbol: string): Promise<RealTimeShareData>
```

Fetch real-time data for a given stock symbol.

#### fetchHistoricalData

```typescript
fetchHistoricalData(options: FetchOptions): Promise<HistoricalShareData[]>
```

Fetch historical data with the specified options.

#### clearCache

```typescript
clearCache(): void
```

Clear the internal cache.

#### getCacheSize

```typescript
getCacheSize(): number
```

Get the number of items in the cache.

## Environment Variables

For security, it's recommended to store API keys in environment variables:

```bash
ALPHA_VANTAGE_API_KEY=your_api_key_here
```

## Error Handling

The library throws errors for various conditions:

- Network errors
- API errors (invalid keys, rate limits, etc.)
- Invalid input parameters
- Data validation failures

Always wrap calls in try/catch blocks:

```typescript
try {
  const data = await fetcher.fetchRealTimeData('AAPL');
  console.log(data);
} catch (error) {
  console.error('Error fetching data:', error.message);
}
```

## Caching

The library includes an in-memory cache with a default TTL of 5 minutes. This reduces the number of API calls and improves performance.

## Testing

Run the test suite with:

```bash
npm test
```

## License

MIT