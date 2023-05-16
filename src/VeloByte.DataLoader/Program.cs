using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VeloByte.DataLoader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(async options =>
                {
                    var host = Host.CreateDefaultBuilder(args)
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton(sp => new DataLoader(options, sp.GetRequiredService<ILogger<DataLoader>>()));
                        })
                        .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Trace))
                        .UseConsoleLifetime()
                        .Build();

                    using (var scope = host.Services.CreateScope())
                    {
                        var sp = scope.ServiceProvider;
                        var dataLoader = sp.GetRequiredService<DataLoader>();
                        await dataLoader.RunAsync();
                    }

                    await host.StartAsync();

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                });
        }
    }
}