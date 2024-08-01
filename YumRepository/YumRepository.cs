
using System.Text;

namespace ArxOne.Yum;

public class YumRepository
{
    private readonly YumRepositorySource _source;

    public YumRepository(YumRepositorySource source)
    {
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
    }

    private object GetConfiguration(GetWithMimeType getWithMimeType)
    {
        var uri = new UriBuilder(_source.GetRequestUri())
        {
            Path = _source.BasePath
        };
        var confText= $@"[{_source.ID}]
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
