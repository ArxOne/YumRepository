﻿using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Rpm;

public record RepomdChecksum
{
    [XAttribute("type")] public string? Type { get; set; }
    [XText] public string? Checksum { get; set; }

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
        Checksum = checksum;
        return true;
    }
}