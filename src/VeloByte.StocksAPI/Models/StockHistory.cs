namespace VeloByte.StocksAPI.Models;

public class StockHistory
{
    /// <summary>
    /// Gets or sets the date of the trading day.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets The opening price of the stock for that day.
    /// </summary>
    public double Open { get; set; }

    /// <summary>
    /// Gets or sets the closing price of the stock for that day.
    /// </summary>

    public double Close { get; set; }

    /// <summary>
    /// Gets or sets the highest price the stock reached during that day.
    /// </summary>
    public double High { get; set; }

    /// <summary>
    /// Gets or sets the lowest price the stock reached during that day.
    /// </summary>
    public double Low { get; set; }

    /// <summary>
    /// Gets or sets the number of shares of that stock that were traded during that day.
    /// </summary>
    public long Volume { get; set; }

    /// <summary>
    /// Gets or sets any dividends paid out on that day.
    /// </summary>
    public double Dividends { get; set; }

    /// <summary>
    /// Gets or sets any stock splits that occurred on that day.
    /// </summary>
    public double StockSplits { get; set; }
}