using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

[XElement("repomd", "http://linux.duke.edu/metadata/repo")]
public class Repomd
{
    [XElement("revision")]
    public long Revision { get; init; }

    [XArrayItem("data")]
    public RepomdData[] Data { get; init; }
}