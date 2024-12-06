using ArxOne.Yum.Utility;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Version
{
    [XAttribute("ver")] public string Ver { get; init; }
    [XAttribute("epoch")] public long? Epoch { get; init; }
    [XAttribute("rel")] public int Rel { get; init; }

    public Version(IReadOnlyDictionary<string, object?> header)
    {
        Ver = header.GetTag<string>("version")!;
        //Epoch = header.GetTag<long?>("buildtime"); //-- a test to avoid error on Fedora upgrades
        Rel = header.GetTag<int>("release");
    }
}
