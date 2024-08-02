using ArxOne.Yum.Xml;

namespace ArxOne.Yum.Rpm;

public record RpmInfoLocation([property: XAttribute("href")] string Href);
