using Microsoft.Extensions.Options;

namespace WhereIsService;

internal sealed class WindowsBackgroundService : BackgroundService
{
    private readonly IOptionsMonitor<Settings> _settings;
    private readonly DateTimeService _dateTimeService;
    private readonly FileSystemWatcherService _fileSystemWatcherService;
    private readonly IndexingService _indexingService;
    private readonly ILogger _logger;

    public WindowsBackgroundService(
        IOptionsMonitor<Settings> settings,
        DateTimeService dateTimeService,
        FileSystemWatcherService fileSystemWatcherService,
        IndexingService indexingService,
        ILoggerFactory loggerFactory)
    {
        _settings = settings;
        _dateTimeService = dateTimeService;
        _fileSystemWatcherService = fileSystemWatcherService;
        _indexingService = indexingService;
        _logger = loggerFactory.CreateLogger("WhereIsService");
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(new EventId(1000, "Service Start"), "Service Started");

        try
        {
            _fileSystemWatcherService.InitFileSystemWatchers(
                watchFolders: _settings.CurrentValue.WatchFolders,
                onChange: (type, e) => {
                    _logger.LogDebug(new EventId(2000, "File Change"), "{Type}: {FilePath}", type, e.FullPath);

                    var fileInfo = new FileInfo(e.FullPath);

                    _indexingService.UpdateFile(File.From(fileInfo));
                });

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);

            /*
            Terminates this process and returns an exit code to the operating system.

            This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            performs one of two scenarios:

            1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            2. When set to "StopHost": will cleanly stop the host, and log errors.

            In order for the Windows Service Management system to leverage configured
            recovery options, we need to terminate the process with a non-zero exit code.
            */

            Environment.Exit(1);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);

        _logger.LogInformation(new EventId(3000, "Service Stop"), "Service Stopped");
    }
}
