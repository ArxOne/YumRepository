using ArxOne.Yum.Utility;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Format
{
    [XElement("license", "http://linux.duke.edu/metadata/rpm")]
    public string? License { get; init; }
    [XElement("vendor", "http://linux.duke.edu/metadata/rpm")]
    public string? Vendor { get; init; }
    [XElement("group", "http://linux.duke.edu/metadata/rpm")]
    public string? Group { get; init; }
    [XElement("buildhost", "http://linux.duke.edu/metadata/rpm")]
    public string? Buildhost { get; init; }
    [XElement("sourcerpm", "http://linux.duke.edu/metadata/rpm")]
    public string? SourceRpm { get; init; }
    [XElement("provides", "http://linux.duke.edu/metadata/rpm")]
    public Entry[]? Provides { get; init; }
    [XElement("requires", "http://linux.duke.edu/metadata/rpm")]
    public Entry[]? Requires { get; init; }
    [XElement("conflicts", "http://linux.duke.edu/metadata/rpm")]
    public Entry[]? Conflicts { get; init; }
    [XArrayItem("file")]
    public string[]? Files { get; init; }

    public Format(IReadOnlyDictionary<string, object?> header)
    {
        License = header.GetTag<string>("license");
        Vendor = header.GetTag<string>("vendor");
        Group = header.GetTag<string>("group");
        Buildhost = header.GetTag<string>("buildhost");
        SourceRpm = header.GetTag<string>("sourcerpm");
        Requires = GetEntries(header, "require");
        Provides = GetEntries(header, "provide");
        Conflicts = GetEntries(header, "conflict");
    }

    private Entry[]? GetEntries(IReadOnlyDictionary<string, object?> header, string baseName)
    {
        if (header.GetTag<string[]>(baseName + "name") is not { } names)
            return null;
        var ver = header.GetTag<string[]>(baseName + "version");
        return GetEntries(names, ver).ToArray();
    }

    private static IEnumerable<Entry> GetEntries(string[] names, string[]? ver)
    {
        for (int index = 0; index < names.Length; index++)
            yield return new Entry(names[index], ver?.ElementAt(index));
    }
}
