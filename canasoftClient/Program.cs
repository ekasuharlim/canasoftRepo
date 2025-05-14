
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables(); 
        })

    .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            services.AddHttpClient<IInventoryApiClient, InventoryApiClientService>(client =>
            {
                var baseAddress = configuration.GetValue<string>("InventoryApi:BaseAddress")
                                    ?? configuration.GetSection("InventoryApi").GetValue<string>("BaseAddress")
                                    ?? Environment.GetEnvironmentVariable("INVENTORY_API_BASE_ADDRESS")
                                    ?? string.Empty;
                client.BaseAddress = new Uri(baseAddress);

                var apiKey = configuration.GetValue<string>("InventoryApi:ApiKey")
                                ?? configuration.GetSection("InventoryApi").GetValue<string>("ApiKey")
                                ?? Environment.GetEnvironmentVariable("INVENTORY_API_KEY")
                                ?? string.Empty;

                client.DefaultRequestHeaders.Add(
                    "X-Api-Key",
                    apiKey
                );
            
            })
            .ConfigurePrimaryHttpMessageHandler( () => {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var handler = new HttpClientHandler();
                if (env != null && env.Equals("development", StringComparison.InvariantCultureIgnoreCase)) {
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
                return handler;
            });

            services.AddSingleton<IInventoryItemSource, FileInventoryItemSource>(
                sp =>
                new FileInventoryItemSource("Data/Inventory.txt")
            );
        }
    )
    .Build();

var apiClient = host.Services.GetRequiredService<IInventoryApiClient>();
var itemSource = host.Services.GetRequiredService<IInventoryItemSource>();


var items = await itemSource.LoadAsync();
foreach (var item in items)
{
    try
    {
        await apiClient.CreateItemAsync(item);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error on {item.ItemId}: {ex.Message}");
    }
}

