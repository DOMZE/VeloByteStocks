namespace VeloByte.StocksAPI;

public class ApplicationOptions
{
    /// <summary>
    /// Gets or sets the SQL connection string
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets whether to use manage identity
    /// </summary>
    public bool UseManageIdentity { get; set; }
}