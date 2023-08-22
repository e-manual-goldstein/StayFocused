using System;

namespace StayFocused.Activities.Handlers
{
    public class FirefoxActivityHandler : IActivityHandler
    {
        public string GetActivityDescription(IntPtr hWnd)
        {
            return WinApi.GetWindowTitle(hWnd);
        }

        //public override string GetDescription(IntPtr hWnd)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
