using System;

namespace StayFocused
{
    public interface IActivity
    {
        string Description { get; }
        int ActivityScore { get; }

        //string GetDescription(IntPtr hWnd);
        void IncrementActivityScore();
    }
}
