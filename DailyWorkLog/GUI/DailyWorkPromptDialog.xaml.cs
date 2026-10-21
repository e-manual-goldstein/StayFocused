using System.Globalization;
using System.Windows;
using System.Windows.Media;
using DailyWorkLog.Services;
using Microsoft.Extensions.Logging;

namespace DailyWorkLog.GUI;

public partial class DailyWorkPromptDialog : Window
{
    private readonly IWorkItemService _workItemService;
    private readonly ILogger<DailyWorkPromptDialog> _logger;

    public int SuccessfulSaveCount { get; private set; }

    public DailyWorkPromptDialog(
        IWorkItemService workItemService,
        double defaultCompletedWorkHours,
        ILogger<DailyWorkPromptDialog> logger)
    {
        _workItemService = workItemService;
        _logger = logger;
        InitializeComponent();
        CompletedWorkTextBox.Text = defaultCompletedWorkHours.ToString("0.##", CultureInfo.InvariantCulture);
        WorkTextBox.Focus();
        _logger.LogDebug(
            "Dialog opened with default completed work {CompletedWorkHours}",
            defaultCompletedWorkHours);
    }

    private async void OnOkClick(object sender, RoutedEventArgs e)
    {
        _logger.LogDebug("OK clicked");
        if (!await TrySaveAsync(saveMode: "OK"))
            return;

        DialogResult = true;
        Close();
    }

    private async void OnSaveAndAddAnotherClick(object sender, RoutedEventArgs e)
    {
        _logger.LogDebug("Save and Add Another clicked");
        await TrySaveAsync(saveMode: "SaveAndAddAnother", clearForAnother: true);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        _logger.LogInformation(
            "Cancel clicked (successful saves this session: {SuccessfulSaves})",
            SuccessfulSaveCount);
        DialogResult = false;
        Close();
    }

    private async Task<bool> TrySaveAsync(string saveMode, bool clearForAnother = false)
    {
        ClearStatus();

        if (string.IsNullOrWhiteSpace(WorkTextBox.Text))
        {
            _logger.LogWarning("Save rejected ({SaveMode}): work text is empty", saveMode);
            ShowError("Please enter what you worked on today.");
            WorkTextBox.Focus();
            return false;
        }

        if (!WorkItemFieldValues.TryParseCompletedWork(CompletedWorkTextBox.Text, out var completedWork))
        {
            _logger.LogWarning(
                "Save rejected ({SaveMode}): invalid completed work value {CompletedWorkText}",
                saveMode,
                CompletedWorkTextBox.Text);
            ShowError("Completed work must be a non-negative number.");
            CompletedWorkTextBox.Focus();
            CompletedWorkTextBox.SelectAll();
            return false;
        }

        _logger.LogInformation(
            "Saving work item ({SaveMode}, text length {TextLength}, completedWork={CompletedWork})",
            saveMode,
            WorkTextBox.Text.Trim().Length,
            completedWork);

        SetButtonsEnabled(false);
        try
        {
            var workItemId = await _workItemService.CreateTaskAsync(
                WorkTextBox.Text,
                completedWork);
            SuccessfulSaveCount++;
            _logger.LogInformation(
                "Save succeeded ({SaveMode}): work item {WorkItemId} (session total {SuccessfulSaves})",
                saveMode,
                workItemId,
                SuccessfulSaveCount);
            ShowSuccess($"Created work item #{workItemId}.");

            if (clearForAnother)
            {
                WorkTextBox.Clear();
                WorkTextBox.Focus();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save failed ({SaveMode})", saveMode);
            ShowError(ex.Message);
            return false;
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        OkButton.IsEnabled = enabled;
        SaveAndAddAnotherButton.IsEnabled = enabled;
        CancelButton.IsEnabled = enabled;
    }

    private void ClearStatus()
    {
        StatusTextBlock.Text = "";
        StatusTextBlock.Visibility = Visibility.Collapsed;
    }

    private void ShowSuccess(string message)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1B, 0x7A, 0x1B));
        StatusTextBlock.Visibility = Visibility.Visible;
    }

    private void ShowError(string message)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB0, 0x00, 0x20));
        StatusTextBlock.Visibility = Visibility.Visible;
    }
}
