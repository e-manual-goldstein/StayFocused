using System.IO;
using System.Text.Json;

namespace DailyWorkLog.Services;

public class PromptStateStore : IPromptStateStore
{
    private readonly string _stateFilePath;

    public PromptStateStore()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DailyWorkLog");
        Directory.CreateDirectory(folder);
        _stateFilePath = Path.Combine(folder, "prompt-state.json");
    }

    public DateTime? GetLastPromptDate()
    {
        if (!File.Exists(_stateFilePath))
            return null;

        var json = File.ReadAllText(_stateFilePath);
        var state = JsonSerializer.Deserialize<PromptState>(json);
        return state?.LastPromptDate;
    }

    public void SetLastPromptDate(DateTime date)
    {
        var state = new PromptState { LastPromptDate = date.Date };
        var json = JsonSerializer.Serialize(state);
        File.WriteAllText(_stateFilePath, json);
    }

    private sealed class PromptState
    {
        public DateTime LastPromptDate { get; set; }
    }
}
