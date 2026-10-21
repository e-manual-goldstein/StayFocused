using DailyWorkLog.GUI;
using DailyWorkLog.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class WorkPromptCoordinator
{
    private readonly IWorkItemService _workItemService;
    private readonly WorkItemOptions _workItemOptions;
    private readonly IPromptStateStore _promptStateStore;
    private readonly ILogger<WorkPromptCoordinator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private bool _dialogOpen;

    public WorkPromptCoordinator(
        IWorkItemService workItemService,
        IOptions<WorkItemOptions> workItemOptions,
        IPromptStateStore promptStateStore,
        ILogger<WorkPromptCoordinator> logger,
        ILoggerFactory loggerFactory)
    {
        _workItemService = workItemService;
        _workItemOptions = workItemOptions.Value;
        _promptStateStore = promptStateStore;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public Task ShowManualPromptAsync()
    {
        RunPrompt(markPromptedToday: false, source: "manual");
        return Task.CompletedTask;
    }

    public Task ShowScheduledPromptAsync()
    {
        RunPrompt(markPromptedToday: true, source: "scheduled");
        return Task.CompletedTask;
    }

    private void RunPrompt(bool markPromptedToday, string source)
    {
        if (_dialogOpen)
        {
            _logger.LogWarning(
                "Skipping {Source} prompt because a dialog is already open",
                source);
            return;
        }

        _dialogOpen = true;
        try
        {
            _logger.LogInformation(
                "Opening work prompt dialog (source={Source}, markPromptedToday={MarkPromptedToday})",
                source,
                markPromptedToday);

            var defaultCompletedWorkHours = WorkItemFieldValues.ResolveDefaultCompletedWorkHours(
                _workItemOptions.MandatoryFields);
            var dialog = new DailyWorkPromptDialog(
                _workItemService,
                defaultCompletedWorkHours,
                _loggerFactory.CreateLogger<DailyWorkPromptDialog>());
            dialog.ShowDialog();

            _logger.LogInformation(
                "Work prompt dialog closed (DialogResult={DialogResult}, SuccessfulSaves={SuccessfulSaves})",
                dialog.DialogResult,
                dialog.SuccessfulSaveCount);

            if (markPromptedToday)
            {
                _promptStateStore.SetLastPromptDate(DateTime.Today);
                _logger.LogInformation(
                    "Marked daily prompt complete for {Date} (work items saved this session: {SuccessfulSaves})",
                    DateTime.Today.ToString("yyyy-MM-dd"),
                    dialog.SuccessfulSaveCount);
            }
        }
        finally
        {
            _dialogOpen = false;
        }
    }
}
