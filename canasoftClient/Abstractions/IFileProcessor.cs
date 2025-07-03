namespace CanasoftClient.Abstractions;
using Microsoft.Extensions.Logging;
public interface IFileProcessor
{
    string FilePrefix { get; }      
    string TypeName { get; }        
    bool CanProcess(string fileName);
    Task ProcessAsync(string filePath, ILogger logger);
}