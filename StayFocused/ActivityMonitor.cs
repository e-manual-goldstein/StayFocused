using Newtonsoft.Json;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using StayFocused.Api;
using StayFocused.Activities;
using System.Collections.Generic;
using StayFocused.Activities.Handlers;

namespace StayFocused
{
    public class ActivityMonitor : IActivityMonitor
    {
        static BasicActivityHandler _basicActivityHandler = new BasicActivityHandler();
        static InActivity _inactive = new InActivity();

        TaskRunner _activityTask;
        TaskRunner _persistenceTask;
        bool _stationLocked;
        bool _archiveExisting = true;

        private static ConcurrentDictionary<string, Activity> _activities; // Dictionary to store activities and their scores
        private Dictionary<string, IActivityHandler> _handlers = new Dictionary<string, IActivityHandler>();

        string SaveFilePath => $"{DateTime.Now:yyyyMMdd}.json";

        public ActivityMonitor(int activityInterval, int persistenceInterval) 
        {
            _activityTask = new TaskRunner(StayFocused, activityInterval);
            _persistenceTask = new TaskRunner(SaveActivitiesToFile, persistenceInterval);
        }

        public void AddCustomHandler(string processName, IActivityHandler activityHandler)
        {
            if (_handlers.ContainsKey(processName))
            {
                throw new NotImplementedException();
            }
            _handlers[processName] = activityHandler;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                Lock();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Unlock();
            }
        }

        public void Begin()
        {
            InitialiseActivities(SaveFilePath);
            ActivateSessionSwitchHandler();

            _persistenceTask.Begin();
            _activityTask.Begin();
        }

        private void ActivateSessionSwitchHandler()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;            
        }

        private void InitialiseActivities(string saveFilePath)
        {
            if (File.Exists(saveFilePath))
            {
                var text = File.ReadAllText(saveFilePath);
                try
                {
                    Console.WriteLine("Loading activities from file...");
                    _activities = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Activity>>(text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to load existing activities {ex.Message}");
                    Console.WriteLine("Archiving existing file");
                    ArchiveActivities(saveFilePath);
                }
            }
            _activities ??= new();            
        }

        #region Stay Focused

        private void StayFocused()
        {
            
            var activity = GetActivity();
            activity.IncrementActivityScore();

            Console.WriteLine($"{activity.Description} - Score: {activity.ActivityScore}");
            
        }

        private IActivity GetActivity()
        {
            var hWnd = GetForegroundWindow(); 
            if (_stationLocked)
            {
                return _inactive;
            }
            var activeProcess = GetWindowProcessName(hWnd);
            if (_handlers.TryGetValue(activeProcess, out var handler))
            {
                return handler.GetActivity(hWnd);
            }
            return _basicActivityHandler.GetBasicActivity(GetWindowTitle(hWnd));
        }

        private static string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder sb = new StringBuilder(nChars);
            GetWindowText(handle, sb, nChars);
            return sb.ToString();
        }

        // Windows API functions for retrieving the active window's title
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, int nSize);
        
        public static string GetActiveWindowTitleAndProcessName()
        {
            IntPtr hWnd = GetForegroundWindow();
            
            return $"Active Window Title: {GetWindowTitle(hWnd)} Process Name: {GetWindowProcessName(hWnd)}";
        }

        private static string GetWindowProcessName(IntPtr hWnd)
        {
            uint processId;
            GetWindowThreadProcessId(hWnd, out processId);

            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }


        #endregion

        private void SaveActivitiesToFile()
        {
            string saveFilePath = SaveFilePath;
            string json = JsonConvert.SerializeObject(_activities, Formatting.Indented);
            if (_archiveExisting)
            {
                ArchiveActivities(saveFilePath);
                _archiveExisting = false;
            }
            // Write JSON to the file
            File.WriteAllTextAsync(saveFilePath, json);
        }

        private void ArchiveActivities(string saveFilePath)
        {
            if (File.Exists(saveFilePath))
            {
                var archiveDirectory = Path.Combine(Path.GetDirectoryName(saveFilePath), "archive");
                if (!Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }
                Console.WriteLine($"Archiving {saveFilePath}");
                File.Copy(saveFilePath, Path.Combine(archiveDirectory, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetFileName(saveFilePath)));
            }
        }

        internal void Lock()
        {
            _stationLocked = true;
        }

        internal void Unlock()
        {
            _stationLocked = false;
        }
    }
}
