namespace ArxOne.Yum.Xml;

[AttributeUsage(AttributeTargets.Class)]
public class XNamespaceAliasAttribute : Attribute
{
    public string Alias { get; }
    public string Namespace { get; }

    public XNamespaceAliasAttribute(string alias, string @namespace)
    {
        Alias = alias;
        Namespace = @namespace;
    }
}
