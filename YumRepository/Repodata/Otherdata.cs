using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

[XElement("otherdata", "http://linux.duke.edu/metadata/common")]
public class Otherdata
{
    [XAttribute("packages")]
    public int PackagesCount { get; set; }

    [XArrayItem("package")]
    public Package[] Packages { get; set; }

    public Otherdata(IEnumerable<Package> rpmInfo)
    {
        Packages = rpmInfo.ToArray();
        PackagesCount = Packages.Length;
    }
}