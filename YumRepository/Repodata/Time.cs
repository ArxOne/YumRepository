using ArxOne.Yum.Utility;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Time
{
    [XAttribute("file")] public long? File { get; init; }
    [XAttribute("build")] public long? Build { get; init; }

    public Time(IReadOnlyDictionary<string, object?> header)
    {
        File = header.GetTag<long?>("filemtimes");
        Build = header.GetTag<long?>("buildtime");
    }
}