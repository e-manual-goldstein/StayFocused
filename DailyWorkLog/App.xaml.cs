using System.Net.Http;
using System.Windows;
using DailyWorkLog.Options;
using DailyWorkLog.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DailyWorkLog;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;
    private TrayHost? _trayHost;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            _serviceProvider = BuildServices();
            ValidateConfiguration(_serviceProvider);

            _trayHost = _serviceProvider.GetRequiredService<TrayHost>();
            _trayHost.Initialize();

            Current.Exit += OnExit;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                ex.Message,
                "Daily Work Log — configuration error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private static ServiceProvider BuildServices()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.Configure<AzureDevOpsOptions>(configuration.GetSection(AzureDevOpsOptions.SectionName));
        services.Configure<DailyPromptOptions>(configuration.GetSection(DailyPromptOptions.SectionName));
        services.Configure<WorkItemOptions>(configuration.GetSection(WorkItemOptions.SectionName));

        services.AddHttpClient<IAzureDevOpsWorkItemService, AzureDevOpsWorkItemService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseDefaultCredentials = true,
                PreAuthenticate = true
            });
        services.AddSingleton<IPromptStateStore, PromptStateStore>();
        services.AddSingleton<WorkPromptCoordinator>();
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
    }
}
