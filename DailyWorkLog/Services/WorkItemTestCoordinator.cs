using DailyWorkLog.GUI;
using DailyWorkLog.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class WorkItemTestCoordinator
{
    private readonly IWorkItemService _workItemService;
    private readonly AzureDevOpsOptions _adoOptions;
    private readonly ILogger<WorkItemTestCoordinator> _logger;

    public WorkItemTestCoordinator(
        IWorkItemService workItemService,
        IOptions<AzureDevOpsOptions> adoOptions,
        ILogger<WorkItemTestCoordinator> logger)
    {
        _workItemService = workItemService;
        _adoOptions = adoOptions.Value;
        _logger = logger;
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
            _logger.LogInformation("Tray test: fetching work item {WorkItemId}", workItemId);
            var item = await _workItemService.GetWorkItemAsync(workItemId);
            _logger.LogInformation(
                "Tray test succeeded for #{Id} [{Type}] state={State}",
                item.Id,
                item.WorkItemType,
                item.State);
            System.Windows.MessageBox.Show(
                $"#{item.Id} [{item.WorkItemType}]\n{item.Title}\nState: {item.State}",
                "Work item found",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tray test failed for work item {WorkItemId}", workItemId);
            System.Windows.MessageBox.Show(
                ex.Message,
                "Could not get work item",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
}
