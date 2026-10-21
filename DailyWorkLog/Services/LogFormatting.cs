namespace DailyWorkLog.Services;

internal static class LogFormatting
{
    private const int MaxBodyLength = 8000;

    public static string Truncate(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text.Length <= MaxBodyLength
            ? text
            : text[..MaxBodyLength] + "…(truncated)";
    }

    public static string SummarizeWorkItemFields(IReadOnlyDictionary<string, object> fields)
    {
        return string.Join(
            ", ",
            fields.Select(pair => $"{pair.Key}={SummarizeFieldValue(pair.Key, pair.Value)}"));
    }

    private static string SummarizeFieldValue(string fieldName, object value)
    {
        var text = value.ToString() ?? "";
        if (fieldName is WorkItemFields.AssignedTo or WorkItemFields.Tags)
            return $"(length {text.Length})";
        if (text.Length <= 120)
            return text;

        return text[..120] + "…";
    }
}
