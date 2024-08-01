using System.Xml.Serialization;

namespace ArxOne.Yum.Rpm;

public record RpmInfoSize
{
    [XmlAttribute("package")] public long? Package { get; init; }
    [XmlAttribute("installed")] public long? Installed { get; init; }
    [XmlAttribute("archive")] public long? Archive { get; init; }
}