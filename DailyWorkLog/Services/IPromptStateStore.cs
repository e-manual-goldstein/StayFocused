namespace DailyWorkLog.Services;

public interface IPromptStateStore
{
    DateTime? GetLastPromptDate();
    void SetLastPromptDate(DateTime date);
}
