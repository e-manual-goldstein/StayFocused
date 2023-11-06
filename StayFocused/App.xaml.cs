using Microsoft.Extensions.DependencyInjection;
using StayFocused.Activities.Handlers;
using StayFocused.Api;
using StayFocused.Plugins;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Forms.Design;
using Application = System.Windows.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StayFocused.Activities;

namespace StayFocused
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ILogManager _logManager;
        private readonly ServiceProvider _serviceProvider;
        private readonly SystemMenu _systemMenu;
        public App()
        {
            var baseServices = RegisterBaseServices();
            _serviceProvider = WithPlugins(baseServices);
            
            _systemMenu = _serviceProvider.GetService<SystemMenu>();
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            AppDomain.CurrentDomain.ProcessExit += _systemMenu.OnExit;
            
            //Current.Dispatcher.InvokeAsync(Start);
        }

        private ServiceCollection RegisterBaseServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<SystemMenu>();
            services.AddSingleton((options) => new ConfigManager());
            services.AddSingleton<ILogManager, DefaultLogger>();
            services.AddSingleton<ConfigManager>();
            services.AddSingleton<IActivityMonitor, ActivityMonitor>();
            services.AddSingleton<PluginService>();
            ConfigureDataAccess(services);
            return services;
        }

        private ServiceCollection ConfigureDataAccess(ServiceCollection services)
        {
            services.AddDbContext<SFDbContext>(options =>
            {
                var folder = Environment.SpecialFolder.ApplicationData;
                var path = Environment.GetFolderPath(folder);
                Directory.CreateDirectory(Path.Join(path, "\\StayFocused\\"));
                options.UseSqlite($"Data Source={Path.Join(path, "\\StayFocused\\sf.db")}");
            });
            return services;
        }

        private ServiceProvider WithPlugins(ServiceCollection baseServices)
        {
            bool rebuildServices = false;
            var serviceCollection = baseServices.BuildServiceProvider();
            var pluginService = serviceCollection.GetService<PluginService>();
            pluginService.Initialise();
            foreach (var (name, plugin) in pluginService.Plugins)
            {
                plugin.OnPluginLoaded(baseServices);
                rebuildServices = true;
            }
            if (rebuildServices)
            {
                serviceCollection = baseServices.BuildServiceProvider();
            }
            foreach (var (name, plugin) in pluginService.Plugins)
            {
                plugin.OnServicesBuilt(serviceCollection);
            }
            return serviceCollection;
        }

        static readonly string _saveFilePath = $"{DateTime.Now:yyyyMMdd}.json";

        private void OnStartup(object sender, StartupEventArgs e)
        {
            
            //Current.Dispatcher.InvokeAsync(
            Start();
              //  );
        }


        [SupportedOSPlatform("windows")]
        public void Start()
        {
            _systemMenu.Initialise(this);
            _logManager = _serviceProvider.GetService<ILogManager>();
            _serviceProvider.GetService<ConfigManager>().SettingNotFound += HandleMissingConfig;

            _serviceProvider.GetService<SFDbContext>().Database.EnsureCreated();
            StartActivityMonitor();
            

            // Keep the main thread alive until the user presses any key to exit
            //Console.WriteLine("StayFocused is running. Press any key to stop...");
            //Console.ReadKey();

        }

        private void StartActivityMonitor()
        {
            var activityMonitor = _serviceProvider.GetService<IActivityMonitor>();
            activityMonitor.AddCustomHandler("firefox", new FirefoxActivityHandler());
            //activityMonitor.AddCustomHandler("OUTLOOK", new OutlookHandler());
            activityMonitor.Begin();
        }

        private string HandleMissingConfig(string key)
        {
            var inputDialog = new InputDialog($"Please provide a value for the {key} config value");
            var result = inputDialog.ShowDialog();
            if (result == true)
            {
                return inputDialog.InputValue;
            }
            _logManager.Log("Cannot continue without setting");
            Shutdown();
            return string.Empty;
        }
    }
}
