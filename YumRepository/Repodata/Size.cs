using ArxOne.Yum.Utility;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Size
{
    [XAttribute("package")] public long? Package { get; init; }
    [XAttribute("installed")] public long? Installed { get; init; }
    [XAttribute("archive")] public long? Archive { get; init; }

    public Size(IReadOnlyDictionary<string, object?> header, long? rpmSize)
    {
        Archive = header.GetTag<long?>("longarchivesize") ?? header.GetTag<long?>("archivesize");
        Installed = header.GetTag<long?>("longsize") ?? header.GetTag<long?>("size");
        Package = rpmSize;
    }
}