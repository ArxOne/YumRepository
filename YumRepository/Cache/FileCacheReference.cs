namespace ArxOne.Yum.Cache;

public record FileCacheReference(string Name, string Distribution, string? Component = null, string? Arch = null);
