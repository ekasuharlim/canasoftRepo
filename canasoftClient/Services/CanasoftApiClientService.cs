using System.Net.Http.Json;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Services;

public class CanasoftApiClientService : IInventoryApiClient, ISalesItemApiClient
{
    private readonly HttpClient _client;

    public CanasoftApiClientService(HttpClient client)
    {
        _client = client;
    }

    public async Task CreateInventoryItemAsync(CreateInventoryItemRequest request)
    {

        var response = await _client.PostAsJsonAsync("api/integration/inventory", request);
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


    public async Task GetAllInventoryItemAsync()
    {
        var response = await _client.GetAsync("api/InventoryItems");
        Console.WriteLine(response.Content);
    }

    public async Task CreateSalesItemAsync(CreateSalesItemRequest request)
    {
        var response = await _client.PostAsJsonAsync("api/integration/sales", request);
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

}