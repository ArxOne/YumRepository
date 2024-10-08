﻿using System.Xml.Linq;

namespace ArxOne.Yum.Xml;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class XElementAttribute : Attribute
{
    public string Name { get; }
    public string? Namespace { get; }

    public XName GetXName(string defaultNamespace = "") => Namespace is null
        ? XName.Get(Name, defaultNamespace)
        : XName.Get(Name, Namespace);

    public XElementAttribute(string name, string? @namespace = null)
    {
        Name = name;
        Namespace = @namespace;
    }
}