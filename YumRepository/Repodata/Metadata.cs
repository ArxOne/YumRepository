using ArxOne.Yum.Rpm;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

[XElement("metadata", "http://linux.duke.edu/metadata/common")]
public class Metadata
{
    [XAttribute("packages")]
    public int PackagesCount { get; set; }

    [XArrayItem("package")]
    public RpmInfo[] Packages { get; set; }

    public Metadata(IEnumerable<RpmInfo> rpmInfo)
    {
        Packages = rpmInfo.ToArray();
        PackagesCount = Packages.Length;
    }
}