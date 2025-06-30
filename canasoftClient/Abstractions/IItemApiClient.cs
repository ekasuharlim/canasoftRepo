
namespace CanasoftClient.Abstractions;
public interface IItemApiClient<TRequest>
{
    Task CreateItemAsync(TRequest item);
}