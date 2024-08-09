
using System.IO.Compression;
using System.Text.Json.Serialization;

namespace ArxOne.Yum.Cache;

using System.Collections.Generic;
using System.Text.Json;

public static class FileCacheExtensions
{
    private class ObjectToInferredTypesConverter : JsonConverter<object>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number when reader.TryGetInt64(out long l) => l,
                JsonTokenType.Number => reader.GetDouble(),
                JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
                JsonTokenType.String => reader.GetString()!,
                JsonTokenType.StartArray => ReadArray(ref reader, options),
                _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
            };
        }

        private object ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var list = new List<object?>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;
                list.Add(Read(ref reader, null, options));
            }

            if (list.Count == 0)
                return Array.Empty<object>();

            var firstItemType = list[0]?.GetType();
            if (firstItemType is null || list.Skip(1).Any(o => o?.GetType() != firstItemType))
                return list.ToArray();

            var array = Array.CreateInstance(firstItemType, list.Count);
            for (int i = 0; i < list.Count; i++)
                array.SetValue(list[i], i);
            return array;
        }

        public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
        }
    }

    private static string GetSerializedPath(string path)
    {
        return $"{path}.json.gz";
    }

    public static TItem? Get<TItem>(this FileCache? cache, string path)
    {
        var serializedPath = GetSerializedPath(path);
        using var gzipStream = cache?.DoGet(serializedPath);
        if (gzipStream is null)
            return default;
        using var jsonStream = new GZipStream(gzipStream, CompressionMode.Decompress);
        return JsonSerializer.Deserialize<TItem?>(jsonStream, new JsonSerializerOptions { Converters = { new ObjectToInferredTypesConverter() }, PropertyNameCaseInsensitive = true });
    }

    public static void Set<TItem>(this FileCache? cache, string path, TItem item)
    {
        var serializedPath = GetSerializedPath(path);
        cache.DoSet(serializedPath, delegate (Stream gzipStream)
        {
            using var jsonStream = new GZipStream(gzipStream, CompressionLevel.SmallestSize);
            JsonSerializer.Serialize(jsonStream, item);
        });
    }

    public static TItem Get<TItem>(this FileCache cache, string path, Func<string, TItem> load)
    {
        var item = cache.Get<TItem>(path);
        if (item is not null)
            return item;
        item = load(path);
        cache.Set(path, item);
        return item;
    }

    public static void Clear(this FileCache? cache, string path)
    {
        cache?.DoClear(path);
    }
}
