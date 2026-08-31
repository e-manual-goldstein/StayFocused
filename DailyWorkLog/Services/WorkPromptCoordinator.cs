using System.Windows;
using DailyWorkLog.GUI;

namespace DailyWorkLog.Services;

public class WorkPromptCoordinator
{
    private readonly IWorkItemService _workItemService;
    private readonly IPromptStateStore _promptStateStore;
    private bool _dialogOpen;

    public WorkPromptCoordinator(
        IWorkItemService workItemService,
        IPromptStateStore promptStateStore)
    {
        _workItemService = workItemService;
        _promptStateStore = promptStateStore;
    }

    public Task ShowManualPromptAsync()
    {
        return RunPromptAsync(markPromptedToday: false);
    }

    public Task ShowScheduledPromptAsync()
    {
        return RunPromptAsync(markPromptedToday: true);
    }

    private async Task RunPromptAsync(bool markPromptedToday)
    {
        if (_dialogOpen)
            return;

        _dialogOpen = true;
        try
        {
            while (true)
            {
                var dialog = new DailyWorkPromptDialog();
                var accepted = dialog.ShowDialog() == true;

                if (!accepted)
                    break;

                try
                {
                    var workItemId = await _workItemService.CreateTaskAsync(dialog.WorkText);
                    System.Windows.MessageBox.Show(
                        $"Created work item #{workItemId}.",
                        "Daily Work Log",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        ex.Message,
                        "Could not create work item",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            if (markPromptedToday)
                _promptStateStore.SetLastPromptDate(DateTime.Today);
        }
        finally
        {
            _dialogOpen = false;
        }
    }
}
