namespace DailyWorkLog.Services;

internal static class WorkItemFields
{
    public const string AssignedTo = "System.AssignedTo";
    public const string Tags = "System.Tags";
    public const string StartDate = "Microsoft.VSTS.Scheduling.StartDate";
    public const string CompletedWork = "Microsoft.VSTS.Scheduling.CompletedWork";

    public const string CurrentUserToken = "@me";
    public const double DefaultCompletedWorkHours = 7.5;
}
