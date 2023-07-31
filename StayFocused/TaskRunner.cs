using System;
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

        public TaskRunner()
        {
            // Set the default interval to 5000 milliseconds (5 seconds)
            _interval = 5000;
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
            Console.WriteLine(GetActiveWindowTitle());
        }

        // Your other asynchronous methods can be defined here
        // For example:
        // Method to check if the user is currently logged in
        public bool IsUserLoggedIn()
        {
            return Environment.UserInteractive; // Returns true if the user is logged in
        }

        // Method to get the full title of the currently active window
        public string GetActiveWindowTitle()
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

        // ... (rest of the class)
    }
}
