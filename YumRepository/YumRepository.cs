
using System.Collections.Immutable;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using ArxOne.Yum.Repodata;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum;

public class YumRepository
{
    private record RpmInfo(IReadOnlyDictionary<string, object?> Signature, IReadOnlyDictionary<string, object?> Header, string RpmPath)
    {
        public Package GetForMetadata()
        {
            return Package.ForMetadata(Signature, Header, new FileInfo(RpmPath).Length) with { Location = new(ToUriPath(RpmPath)) };
        }
        public Package GetForOtherdata()
        {
            return Package.ForOtherdata(Signature, Header);
        }
    }

    private readonly GetRpmInformation _getRpmInformation;
    private readonly YumRepositorySource _source;

    public YumRepository(YumRepositorySource source, GetRpmInformation getRpmInformation)
    {
        _getRpmInformation = getRpmInformation;
        _source = source with { }; // shallow clone
    }

    /*
     * Routes :
     * {BasePath}/{ConfigName}: the configuration file
     * {BasePath}/repodata/repo.xml: link to other xml files
     * {BasePath}/repodata/*.xml.gz: other xml (gzipped) files
     *
     */
    public IEnumerable<(string Path, Delegate Handler)> GetRoutes(GetWithMimeType getWithMimeType)
    {
        var repomdData = GetRepomdData().ToImmutableArray();
        var repomd = new Repomd
        {
            Revision = DateTimeOffset.Now.ToUnixTimeSeconds(),
            Data = repomdData.Select(r => r.Data).ToArray(),
        };
        yield return ($"{_source.BasePath}/{_source.ConfigName}", () => GetConfiguration(getWithMimeType));
        foreach (var (mdData, bytes) in repomdData)
            yield return ($"{_source.BasePath}/{mdData.Location.Href}", () => getWithMimeType(bytes, "application/gzip"));
        var repomdXml = XWriter.ToBytes(repomd);
        yield return ($"{_source.BasePath}/repodata/repomd.xml", () => getWithMimeType(repomdXml, "application/xml"));
        foreach (var localSource in _source.LocalSources)
            yield return ($"{_source.BasePath}/{ToUriPath(localSource)}/{{package}}.rpm", (string package) => GetRpm(localSource, package, getWithMimeType));
    }

    internal static string ToUriPath(string localSource)
    {
        return localSource.Replace('\\', '/');
    }

    private static object GetRpm(string localSource, string package, GetWithMimeType getWithMimeType)
    {
        var rpmBytes = File.ReadAllBytes(Path.Combine(localSource, $"{package}.rpm"));
        return getWithMimeType(rpmBytes, "application/x-redhat-package-manager");
    }

    private IEnumerable<(RepomdData Data, byte[] Bytes)> GetRepomdData()
    {
        var rpmInfo = GetRpmInfo().ToImmutableArray();
        yield return GetRepomdData(GetPrimaryXml(rpmInfo), "primary");
        yield return GetRepomdData(GetOtherXml(rpmInfo), "other");
    }

    private (RepomdData Data, byte[] Bytes) GetRepomdData(byte[] rawData, string type)
    {
        using var compressedStream = new MemoryStream();
        using (var compressionStream = new GZipStream(compressedStream, CompressionLevel.SmallestSize))
            compressionStream.Write(rawData);
        var compressedData = compressedStream.ToArray();
        var rawHash = new RepomdChecksum(Convert.ToHexString(SHA256.Create().ComputeHash(rawData)).ToLower(), "sha256");
        var compressedHash = new RepomdChecksum(Convert.ToHexString(SHA256.Create().ComputeHash(compressedData)).ToLower(), "sha256");
        return (new RepomdData
        {
            Checksum = compressedHash,
            OpenChecksum = rawHash,
            Size = compressedData.Length,
            OpenSize = rawData.Length,
            Type = type,
            Location = new($"repodata/{type}.xml.gz")
        }, compressedData);
    }

    private byte[] GetPrimaryXml(IEnumerable<RpmInfo> rpmInfo)
    {
        var metadata = new Metadata(rpmInfo.Select(r => r.GetForMetadata()));
        return XWriter.ToBytes(metadata);
    }

    private byte[] GetOtherXml(IEnumerable<RpmInfo> rpmInfo)
    {
        var metadata = new Metadata(rpmInfo.Select(r => r.GetForOtherdata()));
        return XWriter.ToBytes(metadata);
    }

    private IEnumerable<RpmInfo> GetRpmInfo()
    {
        foreach (var localSource in _source.LocalSources)
        {
            foreach (var rpmPath in Directory.GetFiles(localSource, "*.rpm"))
            {
                var (signature, header) = _getRpmInformation(rpmPath);
                yield return new(signature, header, rpmPath);
            }
        }
    }

    private object GetPrimary(GetWithMimeType getWithMimeType)
    {
        return getWithMimeType(null, "applicatiom/xml");
    }

    private object GetConfiguration(GetWithMimeType getWithMimeType)
    {
        var uri = new UriBuilder(_source.GetRequestUri())
        {
            Path = _source.BasePath
        };
        var confText = $@"[{_source.ID}]
name={_source.Name}
baseurl={uri.Uri}
repo_gpgcheck=0
gpgcheck=0
enabled=1
".Replace("\r", "");
        return getWithMimeType(Encoding.UTF8.GetBytes(confText), "text/plain");
    }

    public void Reload()
    {
    }
}
