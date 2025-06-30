using System.Net.Http.Json;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;

namespace CanasoftClient.Services;

public class CanasoftApiClientService : IInventoryApiClient, ISalesItemApiClient
{
    private readonly HttpClient _client;
    private readonly ILogger<CanasoftApiClientService> _logger;

    public CanasoftApiClientService(HttpClient client, ILogger<CanasoftApiClientService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task CreateInventoryItemAsync(CreateInventoryItemRequest request)
    {

        var response = await _client.PostAsJsonAsync("api/integration/inventory", request);
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

    public async Task CreateSalesItemAsync(CreateSalesItemRequest request)
    {
        var response = await _client.PostAsJsonAsync("api/integration/sales", request);
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

}