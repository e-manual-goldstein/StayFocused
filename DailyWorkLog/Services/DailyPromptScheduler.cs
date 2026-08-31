using DailyWorkLog.Options;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class DailyPromptScheduler : IDisposable
{
    private readonly DailyPromptOptions _options;
    private readonly IPromptStateStore _promptStateStore;
    private readonly WorkPromptCoordinator _coordinator;
    private readonly System.Timers.Timer _timer;
    private bool _disposed;

    public DailyPromptScheduler(
        IOptions<DailyPromptOptions> options,
        IPromptStateStore promptStateStore,
        WorkPromptCoordinator coordinator)
    {
        _options = options.Value;
        _promptStateStore = promptStateStore;
        _coordinator = coordinator;

        var intervalMs = Math.Max(1, _options.CheckIntervalSeconds) * 1000;
        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    public void Start()
    {
        _timer.Start();
        CheckAndPrompt();
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        CheckAndPrompt();
    }

    private void CheckAndPrompt()
    {
        if (!IsPromptDue())
            return;

        _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(
            () => _coordinator.ShowScheduledPromptAsync());
    }

    private bool IsPromptDue()
    {
        if (!TimeSpan.TryParse(_options.PromptTime, out var promptTime))
            return false;

        var lastPromptDate = _promptStateStore.GetLastPromptDate();
        if (lastPromptDate.HasValue && lastPromptDate.Value.Date >= DateTime.Today)
            return false;

        return DateTime.Now.TimeOfDay >= promptTime;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _timer.Stop();
        _timer.Dispose();
        _disposed = true;
    }
}
