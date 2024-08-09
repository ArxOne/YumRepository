using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

[XElement("metadata", "http://linux.duke.edu/metadata/common")]
public record Metadata
{
    [XAttribute("packages")]
    public int PackagesCount { get; set; }

    [XArrayItem("package")]
    public Package[] Packages { get; set; }

    public Metadata(IEnumerable<Package> rpmInfo)
    {
        Packages = rpmInfo.ToArray();
        PackagesCount = Packages.Length;
    }
}