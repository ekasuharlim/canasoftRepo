
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        {
            services.AddHttpClient<IInventoryApiClient, InventoryApiClientService>(client =>
            {
                //client.BaseAddress = new Uri("https://myapp.local/");
                client.BaseAddress = new Uri("http://localhost:5700/");
                client.DefaultRequestHeaders.Add(
                    "X-Api-Key",
                    "eyJDb21wYW55Q29kZSI6IkNPMDAxIiwiRXhwaXJlcyI6IjIwMjYtMDUtMTRUMDY6NDM6NTUuODkxMDg1N1oiLCJIYXNoIjoieU5qQWkvQzZLb0lqZVQ2cVJlV2k5aGI5MVdGU0RFUk44aVBySElFSmZsWT0ifQ=="
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

