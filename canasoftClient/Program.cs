
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;
using CanasoftClient.Contracts.Request;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })


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
                new FileInventoryItemSource("Data/Inventory.txt")
            );
            
            services.AddSingleton<IItemSource<CreateSalesItemRequest>, FileSalesItemItemSource>(
                sp =>
                new FileSalesItemItemSource("Data/Sales.txt")
            );

        }
    )
    .Build();

var inventoryApiClient = host.Services.GetRequiredService<IInventoryApiClient>();
var inventoryItemSource = host.Services.GetRequiredService<IItemSource<CreateInventoryItemRequest>>();
var salesItemApiClient = host.Services.GetRequiredService<ISalesItemApiClient>();
var salesItemSource = host.Services.GetRequiredService<IItemSource<CreateSalesItemRequest>>();

//await LoadInventoryItems(inventoryItemSource, inventoryApiClient);
await LoadSalesItems(salesItemSource, salesItemApiClient);

async Task LoadSalesItems(IItemSource<CreateSalesItemRequest> itemSource, ISalesItemApiClient apiClient)
{
    Console.WriteLine("Loading Sales items...");
    var items = await itemSource.LoadAsync();
    foreach (var item in items)
    {
        try
        {
            await apiClient.CreateSalesItemAsync(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error on {item.ItemId}: {ex.Message}");
            PrintInnerExceptions(ex);
        }
    }    
    Console.WriteLine($"Loaded {items.Count()} items.");
}


async Task LoadInventoryItems(IItemSource<CreateInventoryItemRequest> itemSource, IInventoryApiClient apiClient)
{
    Console.WriteLine("Loading inventory items...");
    var items = await itemSource.LoadAsync();
    foreach (var item in items)
    {
        try
        {
            await apiClient.CreateInventoryItemAsync(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error on {item.ItemId}: {ex.Message}");
            PrintInnerExceptions(ex);
        }
    }
    Console.WriteLine($"Loaded {items.Count()} items.");
}


void PrintInnerExceptions(Exception ex, int level = 0)
{
    var indent = new string(' ', level * 4);
    Console.WriteLine($"{indent}Exception: {ex.Message}");
    
    if (ex.InnerException != null)
    {
        PrintInnerExceptions(ex.InnerException, level + 1);
    }
}

