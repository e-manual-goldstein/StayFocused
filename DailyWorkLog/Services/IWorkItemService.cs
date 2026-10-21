using DailyWorkLog.Models;

namespace DailyWorkLog.Services;

public interface IWorkItemService
{
    Task<WorkItemSummary> GetWorkItemAsync(int workItemId, CancellationToken cancellationToken = default);

    Task<int> CreateTaskAsync(
        string userText,
        double? completedWorkHours = null,
        CancellationToken cancellationToken = default);
}
