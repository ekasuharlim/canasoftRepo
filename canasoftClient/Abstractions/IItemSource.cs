public interface IItemSource<T>
{
    Task<IEnumerable<T>> LoadAsync(string filePath);
}