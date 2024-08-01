using System.Xml.Serialization;

namespace ArxOne.Yum.Rpm;

public record RpmInfoChecksum
{
    [XmlAttribute("type")] public string? Type { get; init; }
    [XmlAttribute("pkgid")] public string? Pkgid { get; init; } = "YES";
    [XmlText] public string? Checksum { get; init; }
}