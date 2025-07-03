
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;
using CanasoftClient.Contracts.Request;
using CanasoftClient.Factories;
using Microsoft.Extensions.Logging;
using Serilog;
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

            services.AddHttpClient<InventoryApiClientService>("CanasoftClient");
            services.AddHttpClient<SalesApiClientService>("CanasoftClient");

            services.AddScoped<IItemApiClient<CreateInventoryItemRequest>>(sp => sp.GetRequiredService<InventoryApiClientService>());
            services.AddScoped<IItemApiClient<CreateSalesItemRequest>>(sp => sp.GetRequiredService<SalesApiClientService>());

            services.AddSingleton<IItemSource<CreateInventoryItemRequest>>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<FileInventoryItemSource>>();
                var spliter = config.GetValue<string>("AppSettings:FileSpliter") ?? ";";
                return new FileInventoryItemSource(logger, spliter);
            });

            services.AddSingleton<IItemSource<CreateSalesItemRequest>>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<FileSalesItemSourceService>>();
                var spliter = config.GetValue<string>("AppSettings:FileSpliter") ?? ";";
                return new FileSalesItemSourceService(logger, spliter);
            });

            services.AddSingleton<FileProcessorFactory>();            
        }
    )
    .Build();


var logger = host.Services.GetRequiredService<ILogger<Program>>();
var processorFactory = host.Services.GetRequiredService<FileProcessorFactory>();

var inputDirectory = "Data/input";
var processedDirectory = "Data/processed";
var failedDirectory = "Data/failed";
string fileSpliter = host.Services.GetRequiredService<IConfiguration>().GetValue<string>("AppSettings:FileSpliter") ?? ",";

logger.LogInformation("Starting file processing...");
foreach (var filePath in Directory.GetFiles(inputDirectory))
{    
    var fileName = Path.GetFileName(filePath);

    try
    {
        var processor = processorFactory.GetProcessorForFile(fileName);
        if (processor is null)
        {
            logger.LogWarning("No processor found for file {FileName}. Skipping.", fileName);
            continue;
        }


        var (source, apiClient, typeName) = processor.Value;
        if (source is IItemSource<CreateInventoryItemRequest> inventorySource &&
            apiClient is IItemApiClient<CreateInventoryItemRequest> inventoryApiClient)
        {
            await LoadItems(inventorySource, inventoryApiClient, typeName, filePath);
        }
        else if (source is IItemSource<CreateSalesItemRequest> salesSource &&
                 apiClient is IItemApiClient<CreateSalesItemRequest> salesApiClient)
        {
            await LoadItems(salesSource, salesApiClient, typeName, filePath);
        }
        
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
        File.Move(filePath, Path.Combine(processedDirectory, newFileName));
    }
    catch (Exception ex)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
        File.Move(filePath, Path.Combine(failedDirectory, newFileName));
        logger.LogError(ex, "Error processing file {FileName}", newFileName);
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



