namespace DailyWorkLog.Models;

public class WorkItemSummary
{
    public int Id { get; set; }
    public string WorkItemType { get; set; } = "";
    public string Title { get; set; } = "";
    public string State { get; set; } = "";
}
