using System.Collections;
using System.Reflection;
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

    public static XDocument ToDocument(object o)
    {
        var document = new XDocument();
        var rootNameAttribute = o.GetType().GetCustomAttribute<XElementAttribute>();
        var rootName = rootNameAttribute?.XName ?? o.GetType().Name;
        var rootElement = new XElement(rootName);
        document.Add(rootElement);
        FillElement(rootElement, o);
        return document;
    }

    private static void FillElement(XElement node, object o)
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
                node.SetAttributeValue(attributeName, value.ToString());
                continue;
            }
            if (IsElement(propertyInfo, out var elementName))
            {
                var child = new XElement(elementName);
                FillElement(child, value);
                node.Add(child);
                continue;
            }
            if (IsArrayItem(propertyInfo, out var itemName))
            {
                foreach (var item in (IEnumerable)value)
                {
                    var child = new XElement(itemName);
                    FillElement(child, item);
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

    private static bool IsElement(PropertyInfo propertyInfo, out XName elementName)
    {
        var a = propertyInfo.GetCustomAttribute<XElementAttribute>();
        if (a is null)
        {
            elementName = null;
            return false;
        }
        elementName = a.XName;
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
