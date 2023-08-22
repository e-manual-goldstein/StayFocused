using System;

namespace StayFocused
{
    public interface IActivityHandler
    {
        IActivity GetActivity(IntPtr hWnd);
    }
}
