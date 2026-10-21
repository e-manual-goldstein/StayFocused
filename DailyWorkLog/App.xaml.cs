using System.IO;
using System.Net.Http;
using System.Windows;
using DailyWorkLog.Options;
using DailyWorkLog.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DailyWorkLog;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;
    private TrayHost? _trayHost;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            var configuration = ConfigurationSetup.BuildConfiguration();
            LoggingSetup.Configure(configuration);

            _serviceProvider = BuildServices(configuration);
            ValidateConfiguration(_serviceProvider);

            var logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            var localSettingsPath = Path.Combine(
                AppContext.BaseDirectory,
                ConfigurationSetup.LocalSettingsFileName);
            logger.LogInformation(
                "Daily Work Log starting. Base directory: {BaseDirectory}, local settings: {LocalSettingsPresent} ({LocalSettingsPath}), logs: {LogDirectory}",
                AppContext.BaseDirectory,
                File.Exists(localSettingsPath),
                localSettingsPath,
                LoggingSetup.LogDirectory);

            _trayHost = _serviceProvider.GetRequiredService<TrayHost>();
            _trayHost.Initialize();

            Current.Exit += OnExit;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Startup failed");
            System.Windows.MessageBox.Show(
                ex.Message,
                "Daily Work Log — configuration error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            LoggingSetup.Shutdown();
            Shutdown();
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            Log.Fatal(ex, "Unhandled AppDomain exception (terminating={IsTerminating})", e.IsTerminating);
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled dispatcher exception");
    }

    private static ServiceProvider BuildServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);

        services.AddLogging(builder => builder.AddSerilog(dispose: false));

        services.Configure<AzureDevOpsOptions>(configuration.GetSection(AzureDevOpsOptions.SectionName));
        services.Configure<DailyPromptOptions>(configuration.GetSection(DailyPromptOptions.SectionName));
        services.Configure<WorkItemOptions>(configuration.GetSection(WorkItemOptions.SectionName));

        services.AddHttpClient<IWorkItemService, WorkItemService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseDefaultCredentials = true,
                PreAuthenticate = true
            });
        services.AddHttpClient<CurrentUserResolver>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseDefaultCredentials = true,
                PreAuthenticate = true
            });
        services.AddSingleton<IPromptStateStore, PromptStateStore>();
        services.AddSingleton<WorkPromptCoordinator>();
        services.AddSingleton<WorkItemTestCoordinator>();
        services.AddSingleton<DailyPromptScheduler>();
        services.AddSingleton<TrayHost>();

        return services.BuildServiceProvider();
    }

    private static void ValidateConfiguration(ServiceProvider serviceProvider)
    {
        ConfigurationValidator.Validate(
            serviceProvider.GetRequiredService<IOptions<AzureDevOpsOptions>>(),
            serviceProvider.GetRequiredService<IOptions<WorkItemOptions>>(),
            serviceProvider.GetRequiredService<IOptions<DailyPromptOptions>>());
    }

    private void OnExit(object? sender, ExitEventArgs e)
    {
        _trayHost?.Dispose();
        _serviceProvider?.Dispose();
        LoggingSetup.Shutdown();
    }
}
