using System;

namespace StayFocused.Activities
{
    public class Activity : IActivity
    {
        public int ActivityScore { get; set; }
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }

        //public abstract string GetDescription(IntPtr hWnd);
        public TimeSpan Timespan => TimeSpan.FromSeconds(ActivityScore * Constants.MonitoringIntervalMilliseconds);

        public virtual void IncrementActivityScore()
        {
            ActivityScore++;
        }
    }
}
