using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Runtime.Versioning;

using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using StayFocused.Plugins;
using StayFocused.Api;
using System.IO;
using System.Windows.Forms.Design;
using StayFocused.Activities;
using StayFocused.Activities.Handlers;

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
            _systemMenu = new SystemMenu();
            var baseServices = RegisterBaseServices();
            _serviceProvider = WithPlugins(baseServices);
            
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            AppDomain.CurrentDomain.ProcessExit += _systemMenu.OnExit;
            
            //Current.Dispatcher.InvokeAsync(Start);
        }

        private ServiceCollection RegisterBaseServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton((options) => new ConfigManager());
            services.AddSingleton<ILogManager, DefaultLogger>();
            services.AddSingleton<ConfigManager>();
            services.AddSingleton<IActivityMonitor>(new ActivityMonitor(Constants.MonitoringIntervalMilliseconds, Constants.PersistenceIntervalMilliseconds));
            services.AddSingleton<PluginService>();
            
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


            StartActivityMonitor();
            

            // Keep the main thread alive until the user presses any key to exit
            //Console.WriteLine("StayFocused is running. Press any key to stop...");
            //Console.ReadKey();

        }

        private void StartActivityMonitor()
        {
            var activityMonitor = _serviceProvider.GetService<IActivityMonitor>();
            activityMonitor.AddCustomHandler("firefox", new FirefoxActivityHandler());
            //activityMonitor.AddCustomHandler("msedge", new EdgeActivityHandler());
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
