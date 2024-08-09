namespace ArxOne.Yum;

public record YumRepositorySource
{
    public string BasePath { get; set; }

    public List<string> LocalSources { get; set; }

    public string ID { get; set; }

    public string Name { get; set; }

    public string RepoName { get; set; }

    public Func<Uri> GetRequestUri { get; set; }

    public YumRepositorySource(Func<Uri> getRequestUri, string basePath, params string[] localSources)
    {
        GetRequestUri = getRequestUri;
        BasePath = basePath;
        Name = basePath[(basePath.IndexOf('/') + 1)..];
        ID = Name.Replace("/", "");
        RepoName = ID + ".repo";
        LocalSources = localSources.ToList();
    }
}
