using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StayFocused
{
    public class TaskRunner
    {
        private CancellationTokenSource cancellationTokenSource;
        private int _interval; // The delay interval for the timed task in milliseconds
        private ConcurrentDictionary<string, Activity> _activities; // Dictionary to store activities and their scores

        public TaskRunner()
        {
            // Set the default interval to 5000 milliseconds (5 seconds)
            _interval = 5000;
            _activities = new ConcurrentDictionary<string, Activity>();
        }

        public void SetInterval(int intervalMilliseconds)
        {
            // Update the interval value
            _interval = intervalMilliseconds;
        }

        public void Begin()
        {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    // Perform the timed action (in this case, "Stay Focused" function)
                    StayFocused();

                    // Wait for the specified interval
                    await Task.Delay(_interval);
                }
            });
        }

        public void End()
        {
            cancellationTokenSource?.Cancel();
        }

        private void StayFocused()
        {
            string activeWindowTitle = GetActiveWindowTitle();
            var activity = _activities.GetOrAdd(activeWindowTitle, new Activity());
            activity.IncrementActivityScore();

            Console.WriteLine($"{activeWindowTitle} - Score: {activity.ActivityScore}");
        }

        private string GetActiveWindowTitle()
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

        internal void Report()
        {
            foreach (var (name, activity) in _activities)
            {
                Console.WriteLine($"{name}: {activity.ActivityScore}");
            }
        }
    }
}
