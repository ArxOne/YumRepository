using ArxOne.Yum.Utility;

namespace ArxOne.Yum.Repodata;

using ArxOne.Yum.Xml;

[XElement("entry", "http://linux.duke.edu/metadata/rpm")]
public record Entry([property: XAttribute("name")] string Name);

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
        if (header.GetTag<string[]>("requirename") is { } requireNames)
            Requires = requireNames.Select(n => new Entry(n)).ToArray();
        if (header.GetTag<string[]>("providename") is { } provideName)
            Provides = provideName.Select(n => new Entry(n)).ToArray();
        if (header.GetTag<string[]>("conflictname") is { } conflictName)
            Conflicts = conflictName.Select(n => new Entry(n)).ToArray();
    }
}
