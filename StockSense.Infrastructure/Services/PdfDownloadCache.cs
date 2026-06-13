using System.Collections.Concurrent;
using StockSense.Application.Interfaces;

namespace StockSense.Infrastructure.Services;

public class PdfDownloadCache : IPdfDownloadCache
{
    private static readonly ConcurrentDictionary<string, byte[]> Cache = new();

    public string Store(byte[] data)
    {
        var token = Guid.NewGuid().ToString("N");
        Cache[token] = data;
        return token;
    }

    public byte[]? Retrieve(string token)
    {
        Cache.TryRemove(token, out var data);
        return data;
    }
}
