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
        public App()
        {
            var baseServices = RegisterBaseServices();
            _serviceProvider = WithPlugins(baseServices);
            
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            
            //Current.Dispatcher.InvokeAsync(Start);
        }

        private ServiceCollection RegisterBaseServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton((options) => new ConfigManager());
            services.AddSingleton<ILogManager, DefaultLogger>();
            services.AddSingleton<ConfigManager>();
            services.AddSingleton<IActivityMonitor>(new ActivityMonitor(5000, 60000));
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

        private static NotifyIcon notifyIcon;
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
            InitializeNotifyIcon();
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

        private void InitializeNotifyIcon()
        {
            // Create the NotifyIcon instance
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = GetIcon(); // You can set your custom icon here
            notifyIcon.Text = "StayFocused";
            notifyIcon.Visible = true;

            // Set up a context menu for the NotifyIcon (using ContextMenuStrip)
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Exit", null, OnExit);
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // Add a click event handler for the NotifyIcon (optional)
            notifyIcon.Click += OnNotifyIconClick;

            // Add a double-click event handler for the NotifyIcon (optional)
            notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
        }

        private Icon GetIcon()
        {
            using (FileStream fileStream = new FileStream("icon.ico", FileMode.Open, FileAccess.Read))
            {
                // Create the Icon from the FileStream
                return new Icon(fileStream);                
            }
        }

        private void OnNotifyIconClick(object sender, EventArgs e)
        {
            // Handle click event here (e.g., show a tooltip or perform an action)
            //notifyIcon.ShowBalloonTip(1000, "StayFocused", "Application is running!", ToolTipIcon.Info);
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            // Handle double-click event here (e.g., open a window or perform an action)
            // In this example, we'll exit the application on double-click
            Shutdown();
        }

        private void OnExit(object sender, EventArgs e)
        {
            // Clean up resources and exit the application when "Exit" is clicked from the context menu
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            
        }


    }
}
