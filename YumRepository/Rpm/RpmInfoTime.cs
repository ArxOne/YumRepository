using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Rpm;

public record RpmInfoTime
{
    [XAttribute("file")] public long? File { get; init; }
    [XAttribute("build")] public long? Build { get; init; }

    public RpmInfoTime(IReadOnlyDictionary<string, object?> header)
    {
        File = header.GetTag<long?>("filemtimes");
        Build = header.GetTag<long?>("buildtime");
    }
}