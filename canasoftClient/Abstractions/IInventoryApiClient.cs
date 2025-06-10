using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Abstractions;

public interface IInventoryApiClient
{
    Task CreateInventoryItemAsync(CreateInventoryItemRequest request);
    Task GetAllInventoryItemAsync();
}

