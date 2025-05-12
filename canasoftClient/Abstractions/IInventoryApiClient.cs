namespace CanasoftClient.Abstractions;

public interface IInventoryApiClient
{
    Task CreateItemAsync();
    Task GetAllItemAsync();
}

