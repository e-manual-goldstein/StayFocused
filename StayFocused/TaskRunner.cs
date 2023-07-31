using System;
using System.Threading;
using System.Threading.Tasks;

namespace StayFocused
{
    public class TaskRunner
    {
        private CancellationTokenSource cancellationTokenSource;
        private int _interval; // The delay interval for the timed task in milliseconds

        Action TaskAction { get; set; }

        public TaskRunner(Action taskAction, int interval = 5000)
        {
            // Set the default interval to 5000 milliseconds (5 seconds)
            _interval = interval;
            TaskAction = taskAction;
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
                    // Execute the task action
                    TaskAction?.Invoke();

                    // Wait for the specified interval
                    await Task.Delay(_interval);
                }
            });
        }

        public void End()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
