namespace DailyWorkLog.Options;

public class WorkItemOptions
{
    public const string SectionName = "WorkItem";

    public string WorkItemType { get; set; } = "Task";
    public string UserTextField { get; set; } = "System.Title";
    public Dictionary<string, string> MandatoryFields { get; set; } = new();
}
