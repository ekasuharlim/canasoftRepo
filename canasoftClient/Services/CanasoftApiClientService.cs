using System.Net.Http.Json;
using CanasoftClient.Abstractions;
using CanasoftClient.Contracts.Request;
using Microsoft.Extensions.Logging;

namespace CanasoftClient.Services;

public abstract class CanasoftApiClientService 
{
    protected readonly HttpClient _client;
    protected readonly ILogger<CanasoftApiClientService> _logger;

    public CanasoftApiClientService(HttpClient client, ILogger<CanasoftApiClientService> logger)
    {
        _client = client;
        _logger = logger;
    }

}