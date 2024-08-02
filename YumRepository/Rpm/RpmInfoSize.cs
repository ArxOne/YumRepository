using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Rpm;

public record RpmInfoSize
{
    [XAttribute("package")] public long? Package { get; init; }
    [XAttribute("installed")] public long? Installed { get; init; }
    [XAttribute("archive")] public long? Archive { get; init; }

    public RpmInfoSize(IReadOnlyDictionary<string, object?> header, long? rpmSize)
    {
        Archive = header.GetTag<long?>("longarchivesize") ?? header.GetTag<long?>("archivesize");
        Installed = header.GetTag<long?>("longsize") ?? header.GetTag<long?>("size");
        Package = rpmSize;
    }
}