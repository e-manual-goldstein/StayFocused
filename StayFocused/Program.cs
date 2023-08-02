using Microsoft.Win32;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static System.Net.Mime.MediaTypeNames;

namespace StayFocused
{
    public class Program
    {
        static ActivityMonitor _activityMonitor;
        static ConfigManager _configManager;

        [SupportedOSPlatform("windows")]
        public static async Task Main(string[] args)
        {
            _configManager = new ConfigManager();
            InitializeNotifyIcon();
            _activityMonitor = new ActivityMonitor(5000, 60000);
            ActivateSessionSwitchHandler();
            
            await _activityMonitor.BeginAsync();

            // Keep the main thread alive until the user presses any key to exit
            Console.WriteLine("StayFocused is running. Press any key to stop...");
            Console.ReadKey();

        }

        [SupportedOSPlatform("windows")]
        private static void ActivateSessionSwitchHandler()
        {
            //SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);            
        }

        [SupportedOSPlatform("windows")]
        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Console.WriteLine($"Session Switch: {e.Reason}");
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _activityMonitor.Lock();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _activityMonitor.Unlock();
            }
        }

        private static void InitializeNotifyIcon()
        {
            // Create the NotifyIcon instance
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Application; // You can set your custom icon here
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

        private static void OnNotifyIconClick(object sender, EventArgs e)
        {
            // Handle click event here (e.g., show a tooltip or perform an action)
            notifyIcon.ShowBalloonTip(1000, "StayFocused", "Application is running!", ToolTipIcon.Info);
        }

        private static void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            // Handle double-click event here (e.g., open a window or perform an action)
            // In this example, we'll exit the application on double-click
            Application.Exit();
        }

        private static void OnExit(object sender, EventArgs e)
        {
            // Clean up resources and exit the application when "Exit" is clicked from the context menu
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
    }
}
