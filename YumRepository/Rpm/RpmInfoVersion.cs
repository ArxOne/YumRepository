using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Rpm;

public record RpmInfoVersion
{
    [XAttribute("ver")] public string Ver { get; init; }
    [XAttribute("epoch")] public long? Epoch { get; init; }
    [XAttribute("rel")] public int Rel { get; init; }

    public RpmInfoVersion(IReadOnlyDictionary<string, object?> header)
    {
        Ver = header.GetTag<string>("version")!;
        Epoch = header.GetTag<long?>("buildtime");
        Rel = header.GetTag<int>("release");
    }
}