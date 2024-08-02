using ArxOne.Yum.Utility;

namespace ArxOne.Yum.Rpm;

internal static class RpmDictionaryReader
{
    public static TValue? GetTag<TValue>(this IReadOnlyDictionary<string, object?> tags, string tag)
    {
        return (TValue?)GetTag(tags, tag, typeof(TValue));
    }

    public static object? GetTag(this IReadOnlyDictionary<string, object?> tags, string tag, Type valueType)
    {
        if (tags.TryGetValue(tag, out var value) || tags.TryGetValue("RPMTAG_" + tag, out value))
        {
            if (value is null)
                return null;
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                valueType = valueType.GetGenericArguments()[0];
            if (valueType == typeof(string))
            {
                if (value.GetType() == typeof(string[]))
                    return string.Join(Environment.NewLine, (IEnumerable<string>)value);
            }
            return Convert.ChangeType(value, valueType);
        }

        return valueType.CreateDefault();
    }
}
