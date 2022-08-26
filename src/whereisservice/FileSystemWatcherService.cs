namespace WhereIsService;

internal class FileSystemWatcherService : IDisposable
{
    private readonly Stack<FileSystemWatcher> _watchers = new();

    public void InitFileSystemWatchers(
        string[] watchFolders,
        Action<string, FileSystemEventArgs> onChange)
    {
        foreach (var folder in watchFolders)
        {
            _watchers.Push(CreateWatcher(folder, onChange));
        }
    }

    private static FileSystemWatcher CreateWatcher(string path, Action<string, FileSystemEventArgs> onChange)
    {
        var watcher = new FileSystemWatcher {
            Path = path,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Created += (sender, e) => onChange("Created", e);
        watcher.Changed += (sender, e) => onChange("Changed", e);
        watcher.Deleted += (sender, e) => onChange("Deleted", e);
        watcher.Renamed += (sender, e) => onChange("Renamed", e);

        return watcher;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            while (_watchers.Count > 0)
            {
                _watchers.Pop().Dispose();
            }
        }
    }
}
