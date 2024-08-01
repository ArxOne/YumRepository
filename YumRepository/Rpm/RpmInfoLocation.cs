using System.Xml.Serialization;

namespace ArxOne.Yum.Rpm;

public record RpmInfoLocation
{
    [XmlAttribute("href")] public Uri? Href { get; init; }
}