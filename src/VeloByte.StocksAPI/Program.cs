using VeloByte.StocksAPI.Services;

namespace VeloByte.StocksAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddScoped<IStocksService, StocksService>();

            builder.Services.AddOptions<ApplicationOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                options.ConnectionString = configuration.GetConnectionString("VeloByte");
                options.UseManageIdentity = configuration.GetValue<bool>("UseManageIdentity");
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddApplicationInsightsTelemetry();

            var app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}