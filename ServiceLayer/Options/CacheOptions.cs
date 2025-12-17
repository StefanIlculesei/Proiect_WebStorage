namespace ServiceLayer.Options;

public class CacheOptions
{
    public bool Enabled { get; set; } = true;
    public int DefaultTtlSeconds { get; set; } = 120; // 2 minutes
    public int MaxItemsPerSet { get; set; } = 1000; // simple guard
}
