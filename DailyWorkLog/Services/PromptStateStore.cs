using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DailyWorkLog.Services;

public class PromptStateStore : IPromptStateStore
{
    private readonly string _stateFilePath;
    private readonly ILogger<PromptStateStore> _logger;

    public PromptStateStore(ILogger<PromptStateStore> logger)
    {
        _logger = logger;
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DailyWorkLog");
        Directory.CreateDirectory(folder);
        _stateFilePath = Path.Combine(folder, "prompt-state.json");
        _logger.LogDebug("Prompt state file: {StateFilePath}", _stateFilePath);
    }

    public DateTime? GetLastPromptDate()
    {
        if (!File.Exists(_stateFilePath))
        {
            _logger.LogTrace("No prompt state file yet");
            return null;
        }

        var json = File.ReadAllText(_stateFilePath);
        var state = JsonSerializer.Deserialize<PromptState>(json);
        _logger.LogTrace("Read last prompt date: {LastPromptDate}", state?.LastPromptDate);
        return state?.LastPromptDate;
    }

    public void SetLastPromptDate(DateTime date)
    {
        var state = new PromptState { LastPromptDate = date.Date };
        var json = JsonSerializer.Serialize(state);
        File.WriteAllText(_stateFilePath, json);
        _logger.LogInformation(
            "Wrote last prompt date {LastPromptDate} to {StateFilePath}",
            state.LastPromptDate.ToString("yyyy-MM-dd"),
            _stateFilePath);
    }

    private sealed class PromptState
    {
        public DateTime LastPromptDate { get; set; }
    }
}
