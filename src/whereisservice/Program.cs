using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using WhereIsService;

using var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => options.ServiceName = "WhereIs Indexing Service")
    .ConfigureServices((context, services) => {
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);

        services.Configure<Settings>(context.Configuration.GetSection("Settings"));

        services.AddMemoryCache();

        services.AddSingleton<FileSystemWatcherService>();
        services.AddHostedService<WindowsBackgroundService>();
    })
    .Build();

await host.RunAsync();
