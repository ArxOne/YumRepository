using System.Xml.Serialization;

namespace ArxOne.Yum.Rpm;

public record RpmInfoTime
{
    [XmlAttribute("file")] public long? File { get; init; }
    [XmlAttribute("build")] public long? Build { get; init; }
}