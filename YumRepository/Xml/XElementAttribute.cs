using System.Xml.Linq;

namespace ArxOne.Yum.Xml;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class XElementAttribute : Attribute
{
    public string Name { get; }
    public string? Namespace { get; }

    public XName XName => Namespace is null ? Name : XName.Get(Name, Namespace);

    public XElementAttribute(string name, string? @namespace = null)
    {
        Name = name;
        Namespace = @namespace;
    }
}