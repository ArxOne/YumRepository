using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

[XElement("entry", "http://linux.duke.edu/metadata/rpm")]
public record Entry
{
    [XAttribute("name")] public string Name { get; init; }
    [XAttribute("ver")] public string? Ver { get; init; }

    public Entry(string name, string? ver)
    {
        Name = name;
        if (!string.IsNullOrEmpty(ver))
            Ver = ver;
    }
}
