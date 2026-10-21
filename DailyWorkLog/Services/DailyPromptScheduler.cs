using DailyWorkLog.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class DailyPromptScheduler : IDisposable
{
    private readonly DailyPromptOptions _options;
    private readonly IPromptStateStore _promptStateStore;
    private readonly WorkPromptCoordinator _coordinator;
    private readonly ILogger<DailyPromptScheduler> _logger;
    private readonly System.Timers.Timer _timer;
    private bool _disposed;

    public DailyPromptScheduler(
        IOptions<DailyPromptOptions> options,
        IPromptStateStore promptStateStore,
        WorkPromptCoordinator coordinator,
        ILogger<DailyPromptScheduler> logger)
    {
        _options = options.Value;
        _promptStateStore = promptStateStore;
        _coordinator = coordinator;
        _logger = logger;

        var intervalMs = Math.Max(1, _options.CheckIntervalSeconds) * 1000;
        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;

        _logger.LogInformation(
            "Daily prompt scheduler configured (PromptTime={PromptTime}, interval {IntervalSeconds}s)",
            _options.PromptTime,
            _options.CheckIntervalSeconds);
    }

    public void Start()
    {
        _logger.LogInformation("Daily prompt scheduler started");
        _timer.Start();
        CheckAndPrompt();
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        CheckAndPrompt();
    }

    private void CheckAndPrompt()
    {
        if (!IsPromptDue(out var reason))
        {
            _logger.LogTrace("Daily prompt not due: {Reason}", reason);
            return;
        }

        _logger.LogInformation("Daily prompt is due ({Reason}); dispatching to UI thread", reason);
        _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(
            () => _coordinator.ShowScheduledPromptAsync());
    }

    private bool IsPromptDue(out string reason)
    {
        if (!TimeSpan.TryParse(_options.PromptTime, out var promptTime))
        {
            reason = $"invalid PromptTime '{_options.PromptTime}'";
            return false;
        }

        var lastPromptDate = _promptStateStore.GetLastPromptDate();
        if (lastPromptDate.HasValue && lastPromptDate.Value.Date >= DateTime.Today)
        {
            reason = $"already prompted on {lastPromptDate.Value:yyyy-MM-dd}";
            return false;
        }

        if (DateTime.Now.TimeOfDay < promptTime)
        {
            reason = $"before prompt time {promptTime:c} (now {DateTime.Now.TimeOfDay:c})";
            return false;
        }

        reason = $"after prompt time {promptTime:c}";
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Daily prompt scheduler disposing");
        _timer.Stop();
        _timer.Dispose();
        _disposed = true;
    }
}
