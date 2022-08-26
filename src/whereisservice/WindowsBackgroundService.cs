using Microsoft.Extensions.Options;

namespace WhereIsService;

public sealed class WindowsBackgroundService : BackgroundService
{
    private readonly IOptionsMonitor<Settings> _settings;
    private readonly FileSystemWatcherService _service;
    private readonly ILogger<WindowsBackgroundService> _logger;

    public WindowsBackgroundService(
        IOptionsMonitor<Settings> settings,
        FileSystemWatcherService service,
        ILogger<WindowsBackgroundService> logger) =>
        (_settings, _service, _logger) = (settings, service, logger);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            _service.InitFileSystemWatchers(
                watchFolders: _settings.CurrentValue.WatchFolders,
                onChange: (type, e) => _logger.LogInformation(new EventId(1000, "File Change"), "{Type}: {FilePath}", type, e.FullPath));

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

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }
}
