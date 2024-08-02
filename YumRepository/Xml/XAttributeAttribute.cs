namespace ArxOne.Yum.Xml;

[AttributeUsage(AttributeTargets.Property)]
public class XAttributeAttribute: Attribute
{
    public string Name { get; }

    public XAttributeAttribute(string name)
    {
        Name = name;
    }
}