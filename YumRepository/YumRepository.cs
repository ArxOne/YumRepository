
using System.Text;
using ArxOne.Yum.Repodata;
using ArxOne.Yum.Rpm;
using ArxOne.Yum.Xml;

namespace ArxOne.Yum;

public class YumRepository
{
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
        var primaryXml = GetPrimaryXml();
        yield return ($"{_source.BasePath}/{_source.ConfigName}", () => GetConfiguration(getWithMimeType));
        yield return ($"{_source.BasePath}/repodata/primary.xml", () => GetPrimary(getWithMimeType));
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

    private string GetPrimaryXml()
    {
        var metadata = new Metadata(GetRpmInfo());
        using var xmlWriter = new StringWriter();
        XWriter.Write(xmlWriter, metadata);
        return xmlWriter.ToString();
    }

    private IEnumerable<RpmInfo> GetRpmInfo()
    {
        foreach (var localSource in _source.LocalSources)
        {
            foreach (var rpmPath in Directory.GetFiles(localSource, "*.rpm"))
            {
                var (signature, header) = _getRpmInformation(rpmPath);
                var rpmInfo = new RpmInfo(signature, header, new FileInfo(rpmPath).Length) { Location = new(ToUriPath(rpmPath)) };
                yield return rpmInfo;
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
";
        return getWithMimeType(Encoding.UTF8.GetBytes(confText), "text/plain");
    }

    public void Reload()
    {
    }
}
