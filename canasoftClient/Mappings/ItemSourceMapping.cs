using CanasoftClient.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CanasoftClient.Mappings
{
    public class ItemSourceMapping
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDictionary<string, string> _mappings;

        public ItemSourceMapping(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _mappings = configuration.GetSection("FileMappings").GetChildren()
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        public (dynamic Source, dynamic ApiClient, string TypeName, Type RequestType) GetServicesByFileName(string fileName)
        {
            var mappingKey = _mappings.Keys.FirstOrDefault(key => fileName.StartsWith(key, StringComparison.OrdinalIgnoreCase));

            if (mappingKey == null)
            {
                throw new ArgumentException($"No service mapping found for file: {fileName}");
            }

            var requestTypeName = _mappings[mappingKey];
            
            var requestType = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(a => a.GetTypes())
                                .FirstOrDefault(t => t.FullName != null && t.FullName.Equals(requestTypeName, StringComparison.Ordinal));

            if (requestType == null)
            {
                requestType = Type.GetType(requestTypeName, false);
            }

            if (requestType == null)
            {
                throw new TypeLoadException($"Could not load type: {requestTypeName}. Make sure the full type name is correct.");
            }

            var sourceServiceType = typeof(IItemSource<>).MakeGenericType(requestType);
            var apiClientServiceType = typeof(IItemApiClient<>).MakeGenericType(requestType);

            var source = _serviceProvider.GetRequiredService(sourceServiceType);
            var apiClient = _serviceProvider.GetRequiredService(apiClientServiceType);
            
            return (source, apiClient, mappingKey, requestType);
        }
    }
}