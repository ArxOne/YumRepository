using ArxOne.Yum.Rpm;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public class RepomdData
{
    [XAttribute("type")] public string Type { get; init; }
    [XElement("checksum")] public RepomdChecksum Checksum { get; init; }
    [XElement("open-checksum")] public RepomdChecksum OpenChecksum { get; init; }
    [XElement("size")] public long Size { get; init; }
    [XElement("open-size")] public long OpenSize { get; init; }
    [XElement("location")] public RpmInfoLocation Location { get; init; }
    [XElement("timestamp")] public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
