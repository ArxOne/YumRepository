
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using ArxOne.Yum.Cache;
using ArxOne.Yum.Repodata;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum;

public class YumRepository
{
    private sealed record RpmInfo
    {
        public IReadOnlyDictionary<string, object?> Header { get; }
        public string RpmPath { get; }
        public string Sha256Hash { get; }

        public RpmInfo(IReadOnlyDictionary<string, object?> header, string rpmPath, string sha256Hash)
        {
            Header = header.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            RpmPath = rpmPath;
            Sha256Hash = sha256Hash;
        }

        public Package GetForMetadata()
        {
            return Package.ForMetadata(Header, new FileInfo(RpmPath).Length, Sha256Hash) with { Location = new(ToUriPath(RpmPath)) };
        }
        public Package GetForOtherdata()
        {
            return Package.ForOtherdata(Header, Sha256Hash);
        }

        public void Deconstruct(out IReadOnlyDictionary<string, object?> header, out string rpmPath, out string sha256Hash)
        {
            header = Header;
            rpmPath = RpmPath;
            sha256Hash = Sha256Hash;
        }
    }

    private sealed class RepoData
    {
        private readonly IDictionary<string, (RepomdData Data, byte[] Bytes)> _repomdData;

        private byte[]? _repomdXml;
        public byte[] RepomdXml => _repomdXml ??= LoadRepomdXml();

        public RepoData(IEnumerable<(RepomdData Data, byte[] Bytes)> repomdData)
        {
            _repomdData = repomdData.ToDictionary(d => d.Data.ID);
        }

        public byte[]? GetData(string type)
        {
            if (_repomdData.TryGetValue(type, out var t))
                return t.Bytes;
            return null;
        }

        private byte[] LoadRepomdXml()
        {
            var repomd = new Repomd
            {
                Revision = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Data = _repomdData.Values.Select(r => r.Data).ToArray(),
            };
            var repomdXml = XWriter.ToBytes(repomd);
            return repomdXml;
        }
    }

    private readonly GetRpmInformation _getRpmInformation;
    private readonly YumRepositorySource _source;

    private RepoData? _repomdData;
    private RepoData RepomdData => _repomdData ??= new(GetRepomdData());

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
     */

    public IEnumerable<(string Path, Delegate Handler)> GetRoutes(GetWithMimeType getWithMimeType)
    {
        yield return (_source.RepoPath, () => GetConfiguration(getWithMimeType));
        yield return ($"{_source.BasePath}/repodata/{{type}}.xml.gz", (string type) => getWithMimeType(RepomdData.GetData(type), "application/gzip"));
        yield return ($"{_source.BasePath}/repodata/repomd.xml", () => getWithMimeType(RepomdData.RepomdXml, "application/xml"));
        foreach (var localSource in _source.LocalSources)
            yield return ($"{_source.BasePath}/{ToUriPath(localSource)}/{{package}}.rpm", (string package) => GetRpm(localSource, package, getWithMimeType));
    }

    private static string ToUriPath(string localSource)
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

    private static (RepomdData Data, byte[] Bytes) GetRepomdData(byte[] rawData, string type)
    {
        using var compressedStream = new MemoryStream();
        using (var compressionStream = new GZipStream(compressedStream, CompressionLevel.SmallestSize))
            compressionStream.Write(rawData);
        var compressedData = compressedStream.ToArray();
        var rawHash = new RepomdChecksum(Convert.ToHexString(SHA256.Create().ComputeHash(rawData)).ToLower(), "sha256");
        var compressedHash = new RepomdChecksum(Convert.ToHexString(SHA256.Create().ComputeHash(compressedData)).ToLower(), "sha256");
        var repomdData = new RepomdData
        {
            Checksum = compressedHash,
            OpenChecksum = rawHash,
            Size = compressedData.Length,
            OpenSize = rawData.Length,
            Type = type,
        };
        return (repomdData with { Location = new($"repodata/{repomdData.ID}.xml.gz") }, compressedData);
    }

    private static byte[] GetPrimaryXml(IEnumerable<RpmInfo> rpmInfo)
    {
        var metadata = new Metadata(rpmInfo.Select(r => r.GetForMetadata()));
        return XWriter.ToBytes(metadata);
    }

    private static byte[] GetOtherXml(IEnumerable<RpmInfo> rpmInfo)
    {
        var metadata = new Metadata(rpmInfo.Select(r => r.GetForOtherdata()));
        return XWriter.ToBytes(metadata);
    }

    private IEnumerable<string> GetRpmFiles()
    {
        foreach (var localSource in _source.LocalSources)
            foreach (var rpmPath in Directory.GetFiles(localSource, "*.rpm"))
                yield return rpmPath;
    }

    private IEnumerable<RpmInfo> GetRpmInfo()
    {
        return GetRpmFiles().AsParallel().Select(GetRpmInfo);
    }

    private RpmInfo GetRpmInfo(string rpmPath) => _source.Cache.Get(rpmPath, LoadRpmInfo);

    private RpmInfo LoadRpmInfo(string rpmPath)
    {
        var (_, header) = _getRpmInformation(rpmPath);
        using var rpmStream = File.OpenRead(rpmPath);
        var sha256Hash = Convert.ToHexString(SHA256.Create().ComputeHash(rpmStream)).ToLower();
        var rpmInfo = new RpmInfo(header, rpmPath, sha256Hash);
        return rpmInfo;
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
        _repomdData = null;
    }
}
