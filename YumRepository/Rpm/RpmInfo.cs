using System.Xml.Serialization;

namespace ArxOne.Yum.Rpm;

public record RpmInfo
{
    [XmlElement("name")] public string Name { get; init; }
    [XmlElement("arch")] public string Arch { get; init; }
    [XmlElement("version")] public RpmInfoVersion Version { get; init; }
    [XmlElement("checksum")] public RpmInfoChecksum Checksum { get; init; }
    [XmlElement("summary")] public string? Summary { get; init; }
    [XmlElement("description")] public string? Description { get; init; }
    [XmlElement("packager")] public string? Packager { get; init; }
    [XmlElement("url")] public string? Url { get; init; }
    [XmlElement("time")] public RpmInfoTime Time { get; init; }
    [XmlElement("size")] public RpmInfoSize Size { get; init; }

    public RpmInfo(IReadOnlyDictionary<string, object?> signature, IReadOnlyDictionary<string, object?> header)
    {
        Name = header.GetTag<string>("name")!;
        Arch = header.GetTag<string>("arch")!;
    }
}
