using System.Drawing;
using System.Windows.Forms;
using DailyWorkLog.Services;

namespace DailyWorkLog;

public class TrayHost : IDisposable
{
    private readonly WorkPromptCoordinator _coordinator;
    private readonly WorkItemTestCoordinator _testCoordinator;
    private readonly DailyPromptScheduler _scheduler;
    private NotifyIcon? _notifyIcon;
    private bool _disposed;

    public TrayHost(
        WorkPromptCoordinator coordinator,
        WorkItemTestCoordinator testCoordinator,
        DailyPromptScheduler scheduler)
    {
        _coordinator = coordinator;
        _testCoordinator = testCoordinator;
        _scheduler = scheduler;
    }

    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Daily Work Log",
            Visible = true
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Test: get work item by ID", null, OnTestGetWorkItem);
        menu.Items.Add("Log today's work", null, OnLogTodayWork);
        menu.Items.Add("Exit", null, OnExit);
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick += (_, _) => _coordinator.ShowManualPrompt();

        _scheduler.Start();
    }

    private void OnTestGetWorkItem(object? sender, EventArgs e)
    {
        _testCoordinator.GetWorkItem();
    }

    private void OnLogTodayWork(object? sender, EventArgs e)
    {
        _coordinator.ShowManualPrompt();
    }

    private void OnExit(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        _scheduler.Dispose();
        _disposed = true;
    }
}
