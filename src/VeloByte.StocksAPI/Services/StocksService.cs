using System.Data.SqlClient;
using Azure.Core;
using Azure.Identity;
using Dapper;
using Microsoft.Extensions.Options;
using VeloByte.StocksAPI.Models;

namespace VeloByte.StocksAPI.Services;

public class StocksService : IStocksService
{
    private readonly ApplicationOptions _applicationOptions;

    public StocksService(IOptions<ApplicationOptions> applicationOptions)
    {
        _applicationOptions = applicationOptions.Value;
    }

    public async Task<Stock> GetByTickerAsync(string ticker, DateTime startDate, DateTime endDate)
    {
        using (var connection = new SqlConnection(_applicationOptions.ConnectionString))
        {
            if (_applicationOptions.UseManageIdentity)
            {
                var credential = new DefaultAzureCredential();
                var token = await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                connection.AccessToken = token.Token;
            }
            
            await connection.OpenAsync();
            var stockHistory = (await connection.QueryAsync<StockHistory>(
                @"SELECT [TickersHistory].[Date],
                         [TickersHistory].[Open],
                         [TickersHistory].[Close],
                         [TickersHistory].[High],
                         [TickersHistory].[Low],
                         [TickersHistory].[Volume],
                         [TickersHistory].[Dividends],
                         [TickersHistory].[StockSplits]
                 FROM dbo.Tickers
                 INNER JOIN dbo.TickersHistory ON Tickers.Id = TickersHistory.TickerId
                 WHERE Tickers.Ticker = @Ticker
                 AND TickersHistory.Date BETWEEN @StartDate AND @EndDate",
                new { Ticker = ticker, StartDate = startDate, EndDate = endDate }).ConfigureAwait(false)).ToList();

            if (stockHistory.Count == 0)
            {
                throw new StockNotFoundException($"Stock {ticker} was not found or no history found between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");
            }

            return new Stock
            {
                Ticker = ticker.ToUpperInvariant(),
                History = stockHistory.ToList()
            };
        }
    }
}