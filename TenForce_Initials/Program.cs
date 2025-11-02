using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;
using TenForce_Initials.Config;
using TenForce_Initials.Data;
using TenForce_Initials.Output;
using TenForce_Initials.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

var rootCommand = new RootCommand("TenForce Practical - planets & moon average temperatures");

// Output folder option
var outputOption = new Option<string>(
    new[] { "--output", "-o" },
    () => "output",
    "Output folder for CSV/JSON files"
);

// Estimation flag option
var estimateFlag = new Option<bool>(
    new[] { "--estimate-temperatures", "-e" },
    () => false,
    "Force estimator fallback for moon temperatures"
);

rootCommand.AddOption(outputOption);
rootCommand.AddOption(estimateFlag);

rootCommand.SetHandler(async (string output, bool estimate) =>
{
    // create the Host and load appsettings.json
    using IHost host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        })
        .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Information))
        .ConfigureServices((context, services) =>
        {
            IConfiguration configuration = context.Configuration;

            // read API configuration (with key)
            var apiSection = configuration.GetSection("Api");
            string baseUrl = apiSection.GetValue<string>("BaseUrl") ?? "https://api.le-systeme-solaire.net";
            string apiKey = apiSection.GetValue<string>("Key");
            string apiKeyHeader = apiSection.GetValue<string>("KeyHeader") ?? "x-api-key";

            // register app options
            services.AddSingleton(new AppOptions
            {
                OutPutFolder = output,
                ForceEstimator = estimate
            });

            // configure httpClient with API key
            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(30);

                // adding API key from config
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.Add(apiKeyHeader, apiKey);
                    Console.WriteLine($"[INFO] API key header '{apiKeyHeader}' applied from appsettings.json");
                }
                else
                {
                    Console.WriteLine("[WARN] No API key found in appsettings.json!");
                }
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            );

            // registrations of dependencies
            services.AddSingleton<ITemperatureProvider, TemperatureProvider>();
            services.AddSingleton<IDataProcessor, DataProcessor>();
            services.AddSingleton<IOutputWriter, ConsoleWriter>();
            services.AddSingleton<IOutputWriter, FilerWriter>();
            services.AddSingleton<IMoonTemperatureCsvProvider, MoonTemperatureCsvProvider>();
        })
        .Build();

    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting TenForce app; output folder: {out}, estimator forced: {est}", output, estimate);

    try
    {
        var processor = host.Services.GetRequiredService<IDataProcessor>();
        var writers = host.Services.GetServices<IOutputWriter>();

        Console.WriteLine("Loading and processing data...");

        var planets = await processor.LoadPlanetsAsync(CancellationToken.None);

        Console.WriteLine();
        Console.WriteLine($"Found {planets.Count()} planets. Filtering planets with at least one moon...");
        var planetsWithMoons = planets.Where(p => p.Moons.Any()).ToList();
        Console.WriteLine($"Planets with at least one moon: {planetsWithMoons.Count}");

        foreach (var w in writers)
        {
            await w.WritePlanetsWithMoonAveragesAsync(planetsWithMoons, CancellationToken.None);
        }

        Console.WriteLine("Completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled error during execution");
        Console.WriteLine($"Error: {ex.Message}");
        Environment.ExitCode = 1;
    }
    finally
    {
        await host.StopAsync();
    }
}, outputOption, estimateFlag);

return await rootCommand.InvokeAsync(args);
