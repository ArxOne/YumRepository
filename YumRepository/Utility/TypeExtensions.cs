namespace ArxOne.Yum.Utility;

internal static class TypeExtensions
{
    public static object? CreateDefault(this Type type)
    {
        if (type.IsClass)
            return null;
        return Activator.CreateInstance(type);
    }
}