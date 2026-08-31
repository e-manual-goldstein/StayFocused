using System.Globalization;

namespace DailyWorkLog.Services;

internal static class WorkItemFieldValues
{
    public static string TodayStartDate()
    {
        return FormatDateOnly(DateOnly.FromDateTime(DateTime.Today));
    }

    public static string NormalizeTags(object? value)
    {
        if (value is null)
            return "";

        return value.ToString()?.Trim() ?? "";
    }

    public static string NormalizeStartDate(object? value)
    {
        if (value is null)
            return TodayStartDate();

        return value switch
        {
            DateOnly dateOnly => FormatDateOnly(dateOnly),
            DateTime dateTime => FormatDateOnly(DateOnly.FromDateTime(dateTime)),
            string text when DateOnly.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate) => FormatDateOnly(parsedDate),
            string text when DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsedDateTime) => FormatDateOnly(DateOnly.FromDateTime(parsedDateTime)),
            _ => throw new InvalidOperationException(
                $"Work item start date must be a date-only value. Got: {value}")
        };
    }

    private static string FormatDateOnly(DateOnly date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
