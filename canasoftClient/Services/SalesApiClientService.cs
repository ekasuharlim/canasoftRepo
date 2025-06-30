using System.Net.Http.Json;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;

namespace CanasoftClient.Services;

partial class SalesApiClientService : CanasoftApiClientService, ISalesItemApiClient
{
    public SalesApiClientService(HttpClient client, ILogger<CanasoftApiClientService> logger) : base(client, logger)
    {
    }


    public async Task CreateItemAsync(CreateSalesItemRequest item)
    {
        var response = await _client.PostAsJsonAsync("api/integration/sales", item);
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
