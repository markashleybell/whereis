using WhereIsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

using var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => options.ServiceName = "whereis Indexing Service")
    .ConfigureServices((context, services) => {
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);

        services.Configure<Settings>(context.Configuration.GetSection("Settings"));
        services.AddSingleton<FileSystemWatcherService>();
        services.AddHostedService<WindowsBackgroundService>();
    })
    .ConfigureLogging((context, logging) => logging.AddConfiguration(context.Configuration.GetSection("Logging")))
    .Build();

await host.RunAsync();
