using System.Collections;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ArxOne.Yum.Xml;

public static class XWriter
{
    public static void Write(TextWriter writer, object o)
    {
        var xDocument = ToDocument(o);
        using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = true });
        xDocument.WriteTo(xmlWriter);
    }

    public static byte[] ToBytes(object o)
    {
        using var memoryStream = new MemoryStream();
        using var textWriter = new StreamWriter(memoryStream, new UTF8Encoding(false));
        Write(textWriter, o);
        return memoryStream.ToArray();
    }

    public static XDocument ToDocument(object o)
    {
        var document = new XDocument();
        var rootNameAttribute = o.GetType().GetCustomAttribute<XElementAttribute>();
        var rootName = rootNameAttribute?.GetXName() ?? o.GetType().Name;
        var rootElement = new XElement(rootName);
        document.Add(rootElement);
        FillElement(rootElement, o, rootName.NamespaceName);
        return document;
    }

    private static void FillElement(XElement node, object o, string currentNamespace = "")
    {
        if (o.GetType().FullName.StartsWith("System."))
        {
            node.Value = o.ToString();
            return;
        }
        foreach (var propertyInfo in o.GetType().GetProperties())
        {
            var value = propertyInfo.GetValue(o, null);
            if (value is null)
                continue;
            if (IsAttribute(propertyInfo, out var attributeName))
            {
                node.SetAttributeValue(XName.Get(attributeName), value.ToString());
                continue;
            }
            if (IsElement(propertyInfo, currentNamespace, out var elementName))
            {
                var child = new XElement(elementName);
                currentNamespace = elementName.NamespaceName;
                FillElement(child, value, currentNamespace);
                node.Add(child);
                continue;
            }
            if (IsArrayItem(propertyInfo, out var itemName))
            {
                foreach (var item in (IEnumerable)value)
                {
                    var child = new XElement(XName.Get(itemName, currentNamespace));
                    FillElement(child, item, currentNamespace);
                    node.Add(child);
                }
                continue;
            }
            if (IsText(propertyInfo))
            {
                node.Value = value.ToString();
                continue;
            }
        }
    }

    private static bool IsAttribute(PropertyInfo propertyInfo, out string attributeName)
    {
        var a = propertyInfo.GetCustomAttribute<XAttributeAttribute>();
        if (a is null)
        {
            attributeName = null;
            return false;
        }
        attributeName = a.Name;
        return true;
    }

    private static bool IsElement(PropertyInfo propertyInfo, string? currentNamespace, out XName elementName)
    {
        var a = propertyInfo.GetCustomAttribute<XElementAttribute>();
        if (a is null)
        {
            elementName = null;
            return false;
        }
        elementName = a.GetXName(currentNamespace);
        return true;
    }

    private static bool IsArrayItem(PropertyInfo propertyInfo, out string itemName)
    {
        var a = propertyInfo.GetCustomAttribute<XArrayItemAttribute>();
        if (a is null)
        {
            itemName = null;
            return false;
        }
        itemName = a.Name;
        return true;
    }

    private static bool IsText(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttribute<XTextAttribute>() is not null;
    }

}
