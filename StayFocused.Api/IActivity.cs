using System;

namespace StayFocused
{
    public interface IActivity
    {
        string ProcessName { get; }
        int ActivityScore { get; }

        //string GetDescription(IntPtr hWnd);
        void IncrementActivityScore();
    }
}
