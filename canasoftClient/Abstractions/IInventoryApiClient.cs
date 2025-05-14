using CanasoftClient.Contracts.Request;

namespace CanasoftClient.Abstractions;

public interface IInventoryApiClient
{
    Task CreateItemAsync(CreateInventoryItemRequest request);
    Task GetAllItemAsync();
}

