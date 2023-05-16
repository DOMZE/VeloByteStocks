namespace VeloByte.StocksAPI;

public class StockNotFoundException : Exception
{
    public StockNotFoundException()
    {
    }

    public StockNotFoundException(string message) : base(message)
    {
    }

    public StockNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        
    }
}