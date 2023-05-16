namespace VeloByte.StocksAPI.Models;

public class Stock
{
    /// <summary>
    /// Gets or sets the instrument ticker
    /// </summary>
    public string Ticker { get; set; }

    /// <summary>
    /// Gets or sets the history associated
    /// </summary>
    public List<StockHistory> History { get; set; }
}