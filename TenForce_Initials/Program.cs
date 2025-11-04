using Polly;
using Polly.Extensions.Http;
using System.IO.Pipelines;
using System.Net.Http.Headers;
using TenForce_Initials.Config;
using TenForce_Initials.Data;
using TenForce_Initials.Output;
using TenForce_Initials.Services;

// manual argument parsing (replaces System.CommandLine)
string output = "output";
bool estimate = false;

foreach (var arg in args)
{
    if (arg.StartsWith("--output="))
        output = arg.Split('=')[1];
    if (arg.Equals("--estimate-temperatures", StringComparison.OrdinalIgnoreCase))
        estimate = true;
}

Console.WriteLine("=======================================");
Console.WriteLine(" TenForce - Planets & Moon Temperatures ");
Console.WriteLine("=======================================");
Console.WriteLine($"Output folder          : {output}");
Console.WriteLine($"Force temperature calc : {estimate}");
Console.WriteLine();

// hostbuilder
using IHost host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices((context, services) =>
    {
        IConfiguration configuration = context.Configuration;

        // API configuration reading
        var apiSection = configuration.GetSection("Api");
        string baseUrl = apiSection.GetValue<string>("BaseUrl") ?? "https://api.le-systeme-solaire.net";
        string apiKey = apiSection.GetValue<string>("Key");
        string apiKeyHeader = apiSection.GetValue<string>("KeyHeader") ?? "x-api-key";

        // registration of options
        services.AddSingleton(new AppOptions
        {
            OutPutFolder = output,
            ForceEstimator = estimate
        });

        // configuration of HttpClient
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromMinutes(3);

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

        // registration of dependencies
        services.AddSingleton<ITemperatureProvider, TemperatureProvider>();
        services.AddSingleton<IDataProcessor, DataProcessor>();
        services.AddSingleton<IOutputWriter, ConsoleWriter>();
        services.AddSingleton<IOutputWriter, FilerWriter>();
        services.AddSingleton<IMoonTemperatureCsvProvider, MoonTemperatureCsvProvider>();
    })
    .Build();

// process execution
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting TenForce app...");

try
{
    var processor = host.Services.GetRequiredService<IDataProcessor>();
    var writers = host.Services.GetServices<IOutputWriter>();

    Console.WriteLine("Loading and processing data from API...");
    var planets = await processor.LoadPlanetsAsync(CancellationToken.None);

    var planetsWithMoons = planets.Where(p => p.Moons.Any()).ToList();
    Console.WriteLine($"Found {planetsWithMoons.Count} planets with at least one moon.\n");

    foreach (var w in writers)
    {
        await w.WritePlanetsWithMoonAveragesAsync(planetsWithMoons, CancellationToken.None);
    }

    Console.WriteLine("\n Process Completed successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "Unhandled error during execution");
    Console.WriteLine($"\n Error: {ex.Message}");
    Environment.ExitCode = 1;
}
finally
{
    await host.StopAsync();
}