﻿using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Rpm;

public record RpmInfoChecksum
{
    [XAttribute("type")] public string? Type { get; set; }
    [XAttribute("pkgid")] public string? Pkgid { get; init; }
    [XText] public string? Checksum { get; set; }

    public RpmInfoChecksum(IReadOnlyDictionary<string, object?> signature)
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
        Checksum = checksum;
        return true;
    }
}