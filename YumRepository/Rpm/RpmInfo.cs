using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Rpm;

public record RpmInfo
{
    [XAttribute("type")] public string Type { get; init; } = "rpm";

    [XElement("name")] public string Name { get; init; }
    [XElement("arch")] public string Arch { get; init; }
    [XElement("version")] public RpmInfoVersion Version { get; init; }
    [XElement("checksum")] public RpmInfoChecksum Checksum { get; init; }
    [XElement("summary")] public string? Summary { get; init; }
    [XElement("description")] public string? Description { get; init; }
    [XElement("packager")] public string? Packager { get; init; }
    [XElement("url")] public string? Url { get; init; }
    [XElement("time")] public RpmInfoTime Time { get; init; }
    [XElement("size")] public RpmInfoSize Size { get; init; }
    [XElement("location")] public RpmInfoLocation Location { get; init; }

    public RpmInfo(IReadOnlyDictionary<string, object?> signature, IReadOnlyDictionary<string, object?> header, long? rpmSize)
    {
        Name = header.GetTag<string>("name")!;
        Arch = header.GetTag<string>("arch")!;
        Version = new(header);
        Checksum = new(signature);
        Summary = header.GetTag<string>("summary");
        Description = header.GetTag<string>("description");
        Packager = header.GetTag<string>("packager");
        Url = header.GetTag<string>("url");
        Time = new(header);
        Size = new(header, rpmSize);
    }
}
