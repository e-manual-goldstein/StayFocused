using System.Windows;
using DailyWorkLog.GUI;

namespace DailyWorkLog.Services;

public class WorkPromptCoordinator
{
    private readonly IAzureDevOpsWorkItemService _workItemService;
    private readonly IPromptStateStore _promptStateStore;
    private bool _dialogOpen;

    public WorkPromptCoordinator(
        IAzureDevOpsWorkItemService workItemService,
        IPromptStateStore promptStateStore)
    {
        _workItemService = workItemService;
        _promptStateStore = promptStateStore;
    }

    public void ShowManualPrompt()
    {
        RunPrompt(markPromptedToday: false);
    }

    public void ShowScheduledPrompt()
    {
        RunPrompt(markPromptedToday: true);
    }

    private async void RunPrompt(bool markPromptedToday)
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
                    var text = dialog.WorkText;
                    _ = Task.Run(async () =>
                    {
                        var workItemId = await _workItemService.CreateTaskAsync(text);
                        System.Windows.MessageBox.Show(
                            $"Created work item #{workItemId}.",
                            "Daily Work Log",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    });
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
