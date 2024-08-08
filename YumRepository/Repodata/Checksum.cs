using ArxOne.Yum.Utility;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Checksum
{
    [XAttribute("type")] public string? Type { get; set; }
    [XAttribute("pkgid")] public string? Pkgid { get; init; }
    [XText] public string? Value { get; set; }

    public Checksum(IReadOnlyDictionary<string, object?> signature)
    {
        if (TryLoad("sha2", signature) || TryLoad("sha1", signature))
            Pkgid = "YES";
    }

    private bool TryLoad(string algorithm, IReadOnlyDictionary<string, object?> signature)
    {
        var checksum = signature.GetTag<string>(algorithm) ?? signature.GetTag<string>($"{algorithm}header");
        if (checksum is null)
            return false;
        Type = algorithm;
        Value = checksum;
        return true;
    }
}