using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Repodata;

public record Location([property: XAttribute("href")] string Href);
