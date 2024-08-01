
using System.Text;
using ArxOne.Yum.Rpm;

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
        yield return ($"{_source.BasePath}/{_source.ConfigName}", () => GetConfiguration(getWithMimeType));
        yield return ($"{_source.BasePath}/repodata/primary.xml", () => GetPrimary(getWithMimeType));
    }

    private object GetPrimary(GetWithMimeType getWithMimeType)
    {
        foreach (var localSource in _source.LocalSources)
        {
            foreach (var rpmPath in Directory.GetFiles(localSource, "*.rpm"))
            {
                var (signature, header) = _getRpmInformation(rpmPath);
                var rpmInfo = new RpmInfo(signature, header);
            }
        }

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
