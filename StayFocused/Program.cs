using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StayFocused
{
    public class Program
    {
        static readonly string _saveFilePath = $"{DateTime.Now:yyyyMMdd}.json";
        static ActivityMonitor _activityMonitor;

        [SupportedOSPlatform("windows")]
        public static async Task Main(string[] args)
        {
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
