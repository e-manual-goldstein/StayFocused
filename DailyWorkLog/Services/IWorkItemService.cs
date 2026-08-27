using DailyWorkLog.Models;

namespace DailyWorkLog.Services;

public interface IWorkItemService
{
    Task<WorkItemSummary> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);

    Task<int> CreateTaskAsync(string userText, CancellationToken cancellationToken = default);
}
