using System;

namespace StayFocused.Activities
{
    public class Activity : IActivity
    {
        public int ActivityScore { get; private set; }
        public string Description { get; set; }

        //public abstract string GetDescription(IntPtr hWnd);

        public virtual void IncrementActivityScore()
        {
            ActivityScore++;
        }
    }
}
