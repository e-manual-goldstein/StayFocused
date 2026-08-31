using DailyWorkLog.GUI;
using DailyWorkLog.Options;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class WorkItemTestCoordinator
{
    private readonly IWorkItemService _workItemService;
    private readonly AzureDevOpsOptions _adoOptions;

    public WorkItemTestCoordinator(
        IWorkItemService workItemService,
        IOptions<AzureDevOpsOptions> adoOptions)
    {
        _workItemService = workItemService;
        _adoOptions = adoOptions.Value;
    }

    public async Task GetWorkItemAsync()
    {
        int workItemId;

        if (_adoOptions.TestWorkItemId > 0)
        {
            workItemId = _adoOptions.TestWorkItemId;
        }
        else
        {
            var dialog = new WorkItemIdDialog();
            if (dialog.ShowDialog() != true)
                return;

            workItemId = dialog.WorkItemId;
        }

        try
        {
            var item = await _workItemService.GetWorkItemAsync(workItemId);
            System.Windows.MessageBox.Show(
                $"#{item.Id} [{item.WorkItemType}]\n{item.Title}\nState: {item.State}",
                "Work item found",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                ex.Message,
                "Could not get work item",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
}
