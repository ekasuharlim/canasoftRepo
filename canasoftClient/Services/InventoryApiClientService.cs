using System.Net.Http.Json;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;

namespace CanasoftClient.Services;

partial class InventoryApiClientService : CanasoftApiClientService, IInventoryApiClient
{
    public InventoryApiClientService(HttpClient client, ILogger<CanasoftApiClientService> logger) : base(client, logger)
    {
    }


    public async Task CreateItemAsync(CreateInventoryItemRequest item)
    {
        var response = await _client.PostAsJsonAsync("api/integration/inventory", item);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Item created: {Result}", result);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create item: {StatusCode}. Response: {ErrorContent}", response.StatusCode, errorContent);
        }
    }

    public async Task GetAllInventoryItemAsync()
    {
        var response = await _client.GetAsync("api/InventoryItems");
        _logger.LogInformation("{Content}", response.Content);
    }

}
