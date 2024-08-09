using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record RepomdData
{
    [XAttribute("type")] public string Type { get; init; }
    [XElement("checksum")] public RepomdChecksum Checksum { get; init; }
    [XElement("open-checksum")] public RepomdChecksum OpenChecksum { get; init; }
    [XElement("size")] public long Size { get; init; }
    [XElement("open-size")] public long OpenSize { get; init; }
    [XElement("location")] public Location Location { get; init; }
    [XElement("timestamp")] public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public string ID => $"{Checksum.Value}-{Type}";
}
