using System.Net.Http.Json;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Services;

public class InventoryApiClientService : IInventoryApiClient
{
    private readonly HttpClient _client;

    public InventoryApiClientService(HttpClient client)
    {
        _client = client;
    }

    public async Task CreateItemAsync()
    {
        var item = new CreateInventoryItemRequest
        {
            Id = "IT005",
            WarehouseId = "GD001",
            Name = "Example Item",
            Quantity = 10,
            ItemGroupName = "Item group 1"
        };

        var response = await _client.PostAsJsonAsync("api/InventoryItems", item);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Item created:\n" + result);
        }
        else
        {
            Console.WriteLine($"Failed to create item: {response.StatusCode}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task GetAllItemAsync()
    {
        var response = await _client.GetAsync("api/InventoryItems");
        Console.WriteLine(response.Content);
    }
}