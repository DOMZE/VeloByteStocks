using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Logging;

namespace VeloByte.DataLoader;

public class DataLoader
{
    private readonly Options _options;
    private readonly ILogger<DataLoader> _logger;

    public DataLoader(Options options, ILogger<DataLoader> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        try
        {
            var data = ReadFiles();
            await CreateTablesAsync();
            await InsertDataAsync(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the data.");
        }
    }

    private async Task CreateTablesAsync()
    {
        string scriptFileFileContent;
        using (var fileStream = new FileStream(_options.SchemaFilePath, FileMode.Open, FileAccess.Read))
        {
            using (var streamReader = new StreamReader(fileStream))
            {
                scriptFileFileContent = await streamReader.ReadToEndAsync();
            }
        }

        using (var connection = new SqlConnection(_options.ConnectionString))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync(scriptFileFileContent);
        }
    }

    private async Task InsertDataAsync(IDictionary<string, List<StockRecord>> data)
    {
        var sw = new Stopwatch();
        sw.Start();

        _logger.LogInformation("Inserting data into the database");

        var dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(long));
        dataTable.Columns.Add("TickerId", typeof(int));
        dataTable.Columns.Add("Date", typeof(DateTime));
        dataTable.Columns.Add("Open", typeof(double));
        dataTable.Columns.Add("Close", typeof(double));
        dataTable.Columns.Add("High", typeof(double));
        dataTable.Columns.Add("Low", typeof(double));
        dataTable.Columns.Add("Volume", typeof(long));
        dataTable.Columns.Add("Dividends", typeof(double));
        dataTable.Columns.Add("StockSplits", typeof(double));

        using (var connection = new SqlConnection(_options.ConnectionString))
        {
            await connection.OpenAsync();
            foreach (var item in data)
            {
                dataTable.Clear();

                var ticker = item.Key;
                LogVerbose("Inserting {Ticker}", LogLevel.Debug, ticker);
                var id = await connection.QuerySingleAsync<int>("INSERT INTO dbo.Tickers ([Ticker]) OUTPUT INSERTED.Id VALUES(@Ticker)", new { Ticker = ticker });

                foreach (var stock in item.Value)
                {
                    var row = dataTable.NewRow();
                    row["TickerId"] = id;
                    row["Date"] = stock.Date;
                    row["Open"] = stock.Open;
                    row["Close"] = stock.Close;
                    row["High"] = stock.High;
                    row["Low"] = stock.Low;
                    row["Volume"] = stock.Volume;
                    row["Dividends"] = stock.Dividends;
                    row["StockSplits"] = stock.StockSplits;
                    dataTable.Rows.Add(row);
                }

                using (var sqlBulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = "dbo.TickersHistory",
                    BatchSize = Math.Min(1000, dataTable.Rows.Count)
                })
                {
                    await sqlBulkCopy.WriteToServerAsync(dataTable);
                }

                LogVerbose("Inserted {RowCount} rows.", LogLevel.Debug, dataTable.Rows.Count);
            }
        }

        sw.Stop();
        _logger.LogInformation("Inserted {RecordsProcessed} records in {ProcessedTime} minutes", data.Values.Sum(x => x.Count), sw.Elapsed.TotalMinutes);
    }

    private IDictionary<string, List<StockRecord>> ReadFiles()
    {
        var sw = new Stopwatch();
        sw.Start();

        var files = Directory.EnumerateFiles(_options.Directory, "*.csv");
        _logger.LogInformation("Loading files from {DirectoryPath}", _options.Directory);

        var data = new ConcurrentDictionary<string, List<StockRecord>>();
        int processed = 0;

        Parallel.ForEach(files, file =>
        {
            var ticker = Path.GetFileNameWithoutExtension(file);
            var records = new List<StockRecord>();

            LogVerbose("Parsing Ticker {Ticker}", LogLevel.Debug, ticker);
            using (var reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var items = csvReader.GetRecords<StockRecord>();
                    records.AddRange(items);
                }

                data.TryAdd(ticker, records);
                processed++;
            }
        });

        sw.Stop();
        _logger.LogInformation("Processed {FilesProcessed} files in {ProcessedTime} minutes", processed, sw.Elapsed.TotalMinutes);

        return data;
    }

    private void LogVerbose(string message, LogLevel logLevel = LogLevel.Debug, params object[] args)
    {
        if (_options.Verbose && _logger.IsEnabled(logLevel))
        {
            _logger.Log(logLevel, message, args);
        }
    }
}