
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;
using Serilog;
using CanasoftClient.Mappings;
using System.IO;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })

    .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration))


    .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

        
            services.AddHttpClient("CanasoftClient", (sp, client) =>
            {

                var baseAddress = configuration.GetValue<string>("CanasoftApi:BaseAddress")
                                    ?? configuration.GetSection("CanasoftApi").GetValue<string>("BaseAddress")
                                    ?? Environment.GetEnvironmentVariable("CANASOFT_API_BASE_ADDRESS")
                                    ?? string.Empty;
                client.BaseAddress = new Uri(baseAddress);

                var apiKey = configuration.GetValue<string>("CanasoftApi:ApiKey")
                                ?? configuration.GetSection("CanasoftApi").GetValue<string>("ApiKey")
                                ?? Environment.GetEnvironmentVariable("CANASOFT_API_KEY")
                                ?? string.Empty;

                client.DefaultRequestHeaders.Add(
                    "X-Api-Key",
                    apiKey
                );

            })

            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;
                return handler;
            });

            services.AddHttpClient<SalesApiClientService>("CanasoftClient");
            services.AddHttpClient<InventoryApiClientService>("CanasoftClient");

            services.AddScoped<IItemApiClient<CreateInventoryItemRequest>>(sp => sp.GetRequiredService<InventoryApiClientService>());
            services.AddScoped<IItemApiClient<CreateSalesItemRequest>>(sp => sp.GetRequiredService<SalesApiClientService>());
            
            services.AddSingleton<IItemSource<CreateInventoryItemRequest>, FileInventoryItemSource>();
            services.AddSingleton<IItemSource<CreateSalesItemRequest>, FileSalesItemSourceService>();
            services.AddSingleton<ItemSourceMapping>();

        }
    )
    .Build();


var logger = host.Services.GetRequiredService<ILogger<Program>>();
var itemSourceMapping = host.Services.GetRequiredService<ItemSourceMapping>();

var inputDirectory = "Data/input";
var processedDirectory = "Data/processed";
var failedDirectory = "Data/failed";

logger.LogInformation("Starting file processing...");
foreach (var filePath in Directory.GetFiles(inputDirectory))
{
    var fileName = Path.GetFileName(filePath);
    try
    {
        var (source, apiClient, typeName, requestType) = itemSourceMapping.GetServicesByFileName(fileName);
        if (requestType == typeof(CreateInventoryItemRequest))
        {
            await LoadItems((IItemSource<CreateInventoryItemRequest>)source, (IItemApiClient<CreateInventoryItemRequest>)apiClient, typeName, filePath);
        }
        else if (requestType == typeof(CreateSalesItemRequest))
        {
            await LoadItems((IItemSource<CreateSalesItemRequest>)source, (IItemApiClient<CreateSalesItemRequest>)apiClient, typeName, filePath);
        }
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
        File.Move(filePath, Path.Combine(processedDirectory, newFileName));
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing file {FileName}", fileName);
        //File.Move(filePath, Path.Combine(failedDirectory, fileName));
    }
}
logger.LogInformation("File processing completed.");

async Task LoadItems<TRequest>(
    IItemSource<TRequest> itemSource,
    IItemApiClient<TRequest> apiClient,
    string itemTypeName,
    string filePath)
{
    logger.LogInformation("Start sending to inventory API");
    var items = await itemSource.LoadAsync(filePath);
    foreach (var item in items)
    {
        try
        {
            await apiClient.CreateItemAsync(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on loading {itemTypeName} item: {Item}", itemTypeName, item);
        }
    }
    logger.LogInformation("Loaded {ItemCount} {ItemTypeName} items from {FilePath}.", items.Count(), itemTypeName, filePath);
}



