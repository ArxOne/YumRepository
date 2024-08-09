using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Checksum
{
    [XAttribute("type")] public string? Type { get; set; }
    [XAttribute("pkgid")] public string? Pkgid { get; init; }
    [XText] public string? Value { get; set; }

    public Checksum(string hash, string type)
    {
        Type = type;
        Value = hash;
        Pkgid = "YES";
    }
}