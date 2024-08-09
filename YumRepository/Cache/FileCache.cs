
namespace ArxOne.Yum.Cache;

public class FileCache
{
    private readonly string _rootPath;
    private readonly object _lock = new();

    public FileCache(string name, string? rootPath = null)
    {
        _rootPath = rootPath ?? Path.Combine(Path.GetTempPath(), "yum-repository", name);
    }

    private string GetStoragePath(string path)
    {
        var storagePath = Path.Combine(_rootPath, path);
        return storagePath;
    }

    internal Stream DoGet(string path)
    {
        lock (_lock)
        {
            var storagePath = GetStoragePath(path);
            if (File.Exists(storagePath))
                return new MemoryStream(File.ReadAllBytes(storagePath));
            return null;
        }
    }

    internal void DoSet(string path, Action<Stream> action)
    {
        lock (_lock)
        {
            var storagePath = GetStoragePath(path);
            var parentDirectory = Path.GetDirectoryName(storagePath);
            if (!Directory.Exists(parentDirectory))
                Directory.CreateDirectory(parentDirectory);
            using var fileStream = File.Create(storagePath);
            action(fileStream);
        }
    }

    internal void DoClear(string path)
    {
        var storagePath = GetStoragePath(path);
        File.Delete(storagePath);
        try
        {
            for (var parentDirectory = Path.GetDirectoryName(storagePath); parentDirectory != _rootPath; parentDirectory = Path.GetDirectoryName(parentDirectory))
            {
                Directory.Delete(parentDirectory);
            }
        }
        catch (IOException)
        {
            // the directory is not empty, stop
        }
    }
}
