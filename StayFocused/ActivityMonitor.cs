using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
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
            _activityTask = new TaskRunner(StayFocused);
            _persistenceTask = new TaskRunner(SaveActivitiesToFile, persistenceInterval);
        }

        internal async Task BeginAsync()
        {
            await InitialiseActivities();
            _persistenceTask.Begin();
            _activityTask.Begin();
        }

        private async Task InitialiseActivities()
        {
            if (File.Exists(SaveFilePath))
            {
                var text = await File.ReadAllTextAsync(SaveFilePath);
                try
                {
                    Console.WriteLine("Loading activities from file...");
                    _activities = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Activity>>(text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to load existing activities {ex.Message}");
                    Console.WriteLine("Archiving existing file");
                    ArchiveActivities();
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
            // Serialize the _activities dictionary to JSON
            string json = JsonConvert.SerializeObject(_activities, Formatting.Indented);
            if (_archiveExisting)
            {
                ArchiveActivities();
                _archiveExisting = false;
            }
            // Write JSON to the file
            File.WriteAllTextAsync(SaveFilePath, json);
        }

        private void ArchiveActivities()
        {
            if (File.Exists(SaveFilePath))
            {
                var archiveDirectory = Path.Combine(Path.GetDirectoryName(SaveFilePath), "archive");
                if (!Directory.Exists(archiveDirectory))
                {
                    Directory.CreateDirectory(archiveDirectory);
                }
                Console.WriteLine($"Archiving {SaveFilePath}");
                File.Copy(SaveFilePath, Path.Combine(archiveDirectory, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetFileName(SaveFilePath)));
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
