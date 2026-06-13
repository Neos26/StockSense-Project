namespace StockSense.Application.Interfaces;

public interface IPdfDownloadCache
{
    string Store(byte[] data);
    byte[]? Retrieve(string token);
}
