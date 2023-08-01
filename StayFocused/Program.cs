using Microsoft.Win32;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

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
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);            
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
    }
}
