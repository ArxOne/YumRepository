using ArxOne.Yum.Cache;

namespace ArxOne.Yum;

public record YumRepositorySource
{
    public string BasePath { get; init; }

    public List<string> LocalSources { get; init; }

    public string ID { get; init; }

    public string Name { get; init; }

    public string? RepoPath { get; init; }

    public Func<Uri> GetRequestUri { get; init; }

    public FileCache Cache { get; init; }

    public YumRepositorySource(Func<Uri> getRequestUri, string basePath, params string[] localSources)
    {
        GetRequestUri = getRequestUri;
        BasePath = basePath;
        Name = basePath[(basePath.IndexOf('/') + 1)..];
        ID = Name.Replace("/", "");
        RepoPath = $"{basePath}/{ID}.repo";
        LocalSources = localSources.ToList();
    }
}
