using System.Collections.Concurrent;

namespace SimpleLedger.Application;

public class IdempotencyService
{
    private readonly ConcurrentDictionary<string, object> _processedKeys = new();

    public bool IsProcessed(string key)
    {
        return _processedKeys.ContainsKey(key);
    }

    public bool MarkAsProcessed(string key, object result)
    {
        return _processedKeys.TryAdd(key, result);
    }

    public object? GetResult(string key)
    {
        return _processedKeys.TryGetValue(key, out var result) ? result : null;
    }
}