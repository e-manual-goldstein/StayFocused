namespace StayFocused.Api
{
    public interface IActivityMonitor
    {
        void Begin();

        void AddCustomHandler(string processName, IActivityHandler activityHandler);
    }
}
