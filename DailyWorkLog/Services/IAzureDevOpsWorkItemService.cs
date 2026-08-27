namespace DailyWorkLog.Services;

public interface IAzureDevOpsWorkItemService
{
    Task<int> CreateTaskAsync(string userText, CancellationToken cancellationToken = default);
}
