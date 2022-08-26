using WhereIsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

using var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => options.ServiceName = "WhereIs Indexing Service")
    .ConfigureServices((context, services) => {
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);

        services.Configure<Settings>(context.Configuration.GetSection("Settings"));
        services.AddSingleton<FileSystemWatcherService>();
        services.AddHostedService<WindowsBackgroundService>();
    })
    .Build();

await host.RunAsync();
