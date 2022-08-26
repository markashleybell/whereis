using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace WhereIsService;

internal class FileSystemWatcherService : IDisposable
{
    private const int CacheExpiryTimeMs = 1000;

    private readonly Stack<FileSystemWatcher> _watchers = new();

    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    public FileSystemWatcherService(
        IMemoryCache cache,
        ILoggerFactory loggerFactory)
    {
        _cache = cache;
        _logger = loggerFactory.CreateLogger("WhereIsService");
    }

    public void InitFileSystemWatchers(
        string[] watchFolders,
        Action<FileChangeType, FileSystemEventArgs> onChange)
    {
        foreach (var folder in watchFolders)
        {
            _watchers.Push(CreateWatcher(folder, onChange));
        }
    }

    private FileSystemWatcher CreateWatcher(string path, Action<FileChangeType, FileSystemEventArgs> onChange)
    {
        var watcher = new FileSystemWatcher {
            Path = path,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        void onChangeThrottled(FileChangeType type, FileSystemEventArgs e)
        {
            var cs = new CancellationTokenSource(CacheExpiryTimeMs);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove)
                .AddExpirationToken(new CancellationChangeToken(cs.Token))
                .RegisterPostEvictionCallback((_, v, reason, _) => {
                    if (reason != EvictionReason.TokenExpired)
                    {
                        return;
                    }

                    onChange(type, (FileSystemEventArgs)v);
                });

            _cache.Set((type, e.FullPath), e, cacheEntryOptions);
        }

        watcher.Created += (sender, e) => onChangeThrottled(FileChangeType.Created, e);
        watcher.Changed += (sender, e) => onChangeThrottled(FileChangeType.Changed, e);
        watcher.Deleted += (sender, e) => onChangeThrottled(FileChangeType.Deleted, e);
        watcher.Renamed += (sender, e) => onChangeThrottled(FileChangeType.Renamed, e);

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
