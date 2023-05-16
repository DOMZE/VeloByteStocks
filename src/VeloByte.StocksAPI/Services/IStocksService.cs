using VeloByte.StocksAPI.Models;

namespace VeloByte.StocksAPI.Services;

public interface IStocksService
{
    Task<Stock> GetByTickerAsync(string ticker, DateTime startDate, DateTime endDate);
}