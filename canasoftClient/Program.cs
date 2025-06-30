
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


var salesItemApiClient = host.Services.GetRequiredService<IItemApiClient<CreateSalesItemRequest>>();
var inventoryApiClient = host.Services.GetRequiredService<IItemApiClient<CreateInventoryItemRequest>>();

var inventoryItemSource = host.Services.GetRequiredService<IItemSource<CreateInventoryItemRequest>>();
var salesItemSource = host.Services.GetRequiredService<IItemSource<CreateSalesItemRequest>>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();



async Task LoadItems<TRequest>(
    IItemSource<TRequest> itemSource,
    IItemApiClient<TRequest> apiClient,
    string itemTypeName)
{
    logger.LogInformation("Loading {ItemTypeName} items...", itemTypeName);
    var items = await itemSource.LoadAsync();
    foreach (var item in items)
    {
        try
        {

            await apiClient.CreateItemAsync(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on  loading {itemTypeName} item: {Item}", itemTypeName, item);
        }
    }
    logger.LogInformation("Loaded {ItemCount} {ItemTypeName} items.", items.Count(), itemTypeName);
}

await LoadItems<CreateInventoryItemRequest>(inventoryItemSource, inventoryApiClient, "Inventory");
await LoadItems<CreateSalesItemRequest>(salesItemSource, salesItemApiClient, "Sales");



