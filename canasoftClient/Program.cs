
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CanasoftClient.Abstractions;
using CanasoftClient.Services;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddHttpClient<IInventoryApiClient, InventoryApiClientService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5070/");
        });
    })
    .Build();

var apiClient = host.Services.GetRequiredService<IInventoryApiClient>();
await apiClient.CreateItemAsync();