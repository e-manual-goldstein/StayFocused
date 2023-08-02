using Newtonsoft.Json;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused
{
    public class ActivityMonitor
    {
        TaskRunner _activityTask;
        TaskRunner _persistenceTask;
        bool _stationLocked;
        bool _archiveExisting = true;

        private static ConcurrentDictionary<string, Activity> _activities; // Dictionary to store activities and their scores
        
        string SaveFilePath => $"{DateTime.Now:yyyyMMdd}.json";

        public ActivityMonitor(int activityInterval, int persistenceInterval) 
        {
            _activityTask = new TaskRunner(StayFocused, activityInterval);
            _persistenceTask = new TaskRunner(SaveActivitiesToFile, persistenceInterval);
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

        internal void Begin()
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
            var activity = _activities.GetOrAdd(GetActivity(), (key) => new Activity() {  Description = key });
            activity.IncrementActivityScore();

            Console.WriteLine($"{activity.Description} - Score: {activity.ActivityScore}");
            
        }

        private string GetActivity()
        {
            if (_stationLocked)
            {
                return "Inactive";
            }
            return GetActiveWindowTitle();            
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = GetForegroundWindow();
            StringBuilder sb = new StringBuilder(nChars);
            GetWindowText(handle, sb, nChars);
            return sb.ToString();
        }

        // Windows API functions for retrieving the active window's title
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

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
