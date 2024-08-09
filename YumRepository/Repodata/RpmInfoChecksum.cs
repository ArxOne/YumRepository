using ArxOne.Yum.Utility;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record RepomdChecksum
{
    [XAttribute("type")] public string? Type { get; set; }
    [XText] public string? Value { get; set; }

    public RepomdChecksum(string value, string type)
    {
        Value = value;
        Type = type;
    }

    public RepomdChecksum(IReadOnlyDictionary<string, object?> signature)
    {
        _ = TryLoad("sha2", signature) || TryLoad("sha1", signature);
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