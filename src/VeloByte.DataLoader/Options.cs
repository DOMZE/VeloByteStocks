using CommandLine;

namespace VeloByte.DataLoader;

public class Options
{
    [Option('d', "directory", Required = true, HelpText = "Set the directory where the files are located.")]
    public string Directory { get; set; }

    [Option('s', "schema", Required = true, HelpText = "Set the schema file path.")]
    public string SchemaFilePath { get; set; }

    [Option('c', "connectionstring", Required = true, HelpText = "Set the connection string to the SQL database")]
    public string ConnectionString { get; set; }

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
}