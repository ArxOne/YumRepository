using System.Xml.Serialization;

namespace ArxOne.Yum.Rpm;

public record RpmInfoVersion
{
    [XmlAttribute("ver")] public string Ver { get; init; }
    [XmlAttribute("epoch")] public int? Epoch { get; init; }
    [XmlAttribute("rel")] public int Rel { get; init; }
}