using CanasoftClient.Abstractions;

namespace CanasoftClient.Processor;

public class FileProcessorFactory
{
    private readonly IEnumerable<IFileProcessor> _processors;

    public FileProcessorFactory(IEnumerable<IFileProcessor> processors)
    {
        _processors = processors;
    }

    public IFileProcessor? GetProcessorForFile(string fileName) =>
        _processors.FirstOrDefault(p => p.CanProcess(fileName));    
}