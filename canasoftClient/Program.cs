
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;
using Serilog;

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
        
            services.AddHttpClient<CanasoftApiClientService>(client =>
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

            services.AddScoped<IInventoryApiClient>(sp => sp.GetRequiredService<CanasoftApiClientService>());
            services.AddScoped<ISalesItemApiClient>(sp => sp.GetRequiredService<CanasoftApiClientService>());
            
            services.AddSingleton<IItemSource<CreateInventoryItemRequest>, FileInventoryItemSource>(
                sp =>
                new FileInventoryItemSource("Data/Inventory.txt", sp.GetRequiredService<ILogger<FileInventoryItemSource>>()))
            ;
            
            services.AddSingleton<IItemSource<CreateSalesItemRequest>, FileSalesItemSourceService>(
                sp =>
                new FileSalesItemSourceService("Data/Sales.txt", sp.GetRequiredService<ILogger<FileSalesItemSourceService>>()))
            ;

        }
    )
    .Build();

var inventoryApiClient = host.Services.GetRequiredService<IInventoryApiClient>();
var inventoryItemSource = host.Services.GetRequiredService<IItemSource<CreateInventoryItemRequest>>();
var salesItemApiClient = host.Services.GetRequiredService<ISalesItemApiClient>();
var salesItemSource = host.Services.GetRequiredService<IItemSource<CreateSalesItemRequest>>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

await LoadInventoryItems(inventoryItemSource, inventoryApiClient);
await LoadSalesItems(salesItemSource, salesItemApiClient);

async Task LoadSalesItems(IItemSource<CreateSalesItemRequest> itemSource, ISalesItemApiClient apiClient)
{
    logger.LogInformation("Loading Sales items...");
    var items = await itemSource.LoadAsync();
    foreach (var item in items)
    { 
        try
        {
            await apiClient.CreateSalesItemAsync(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on {ItemId}: {ErrorMessage}", item.ItemId, ex.Message);
        }
    }    
    logger.LogInformation("Loaded {ItemCount} items.", items.Count());
}


async Task LoadInventoryItems(IItemSource<CreateInventoryItemRequest> itemSource, IInventoryApiClient apiClient)
{
    logger.LogInformation("Loading inventory items...");
    var items = await itemSource.LoadAsync();
    foreach (var item in items)
    {
        try
        {
            await apiClient.CreateInventoryItemAsync(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on {ItemId}: {ErrorMessage}", item.ItemId, ex.Message);
        }
    }
    logger.LogInformation("Loaded {ItemCount} items.", items.Count());
}

