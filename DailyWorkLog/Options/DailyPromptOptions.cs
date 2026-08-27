namespace DailyWorkLog.Options;

public class DailyPromptOptions
{
    public const string SectionName = "DailyPrompt";

    public string PromptTime { get; set; } = "17:00";
    public int CheckIntervalSeconds { get; set; } = 60;
}
