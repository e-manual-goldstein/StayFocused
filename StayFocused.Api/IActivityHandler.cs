using System;

namespace StayFocused
{
    public interface IActivityHandler
    {
        string GetActivityDescription(IntPtr hWnd);
    }
}
