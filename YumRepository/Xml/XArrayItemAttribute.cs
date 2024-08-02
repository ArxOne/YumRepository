namespace ArxOne.Yum.Xml;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class XArrayItemAttribute : Attribute
{
    public string Name { get; }

    public XArrayItemAttribute(string name)
    {
        Name = name;
    }
}