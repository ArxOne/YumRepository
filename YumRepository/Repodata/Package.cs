using ArxOne.Yum.Utility;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Package
{
    [XAttribute("type")] public string Type { get; init; }
    [XAttribute("pkgid")] public string Pkgid { get; init; }
    [XElement("name")] public string NameElement { get; init; }
    [XElement("arch")] public string ArchElement { get; init; }
    [XAttribute("name")] public string NameAttribute { get; init; }
    [XAttribute("arch")] public string ArchAttribute { get; init; }
    [XElement("version")] public Version Version { get; init; }
    [XElement("checksum")] public Checksum Checksum { get; init; }
    [XElement("summary")] public string? Summary { get; init; }
    [XElement("description")] public string? Description { get; init; }
    [XElement("packager")] public string? Packager { get; init; }
    [XElement("url")] public string? Url { get; init; }
    [XElement("time")] public Time Time { get; init; }
    [XElement("size")] public Size Size { get; init; }
    [XElement("location")] public Location Location { get; init; }

    public static Package ForMetadata(IReadOnlyDictionary<string, object?> header, long? rpmSize, string sha256Hash)
    {
        return new Package
        {
            Type = "rpm",
            NameElement = header.GetTag<string>("name")!,
            ArchElement = header.GetTag<string>("arch")!,
            Version = new(header),
            Checksum = new(sha256Hash, "sha256"),
            Summary = header.GetTag<string>("summary"),
            Description = header.GetTag<string>("description"),
            Packager = header.GetTag<string>("packager"),
            Url = header.GetTag<string>("url"),
            Time = new(header),
            Size = new(header, rpmSize),
        };
    }

    public static Package ForOtherdata(IReadOnlyDictionary<string, object?> header, string sha256Hash)
    {
        return new Package
        {
            Pkgid = sha256Hash,
            NameAttribute = header.GetTag<string>("name")!,
            ArchAttribute = header.GetTag<string>("arch")!,
            Version = new(header),
        };
    }
}
