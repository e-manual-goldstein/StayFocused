using System.Net.Http;
using System.Text;
using System.Text.Json;
using DailyWorkLog.Models;
using DailyWorkLog.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class WorkItemService : IWorkItemService
{
    private readonly HttpClient _httpClient;
    private readonly AzureDevOpsOptions _adoOptions;
    private readonly WorkItemOptions _workItemOptions;
    private readonly CurrentUserResolver _currentUserResolver;
    private readonly ILogger<WorkItemService> _logger;

    public WorkItemService(
        HttpClient httpClient,
        IOptions<AzureDevOpsOptions> adoOptions,
        IOptions<WorkItemOptions> workItemOptions,
        CurrentUserResolver currentUserResolver,
        ILogger<WorkItemService> logger)
    {
        _httpClient = httpClient;
        _adoOptions = adoOptions.Value;
        _workItemOptions = workItemOptions.Value;
        _currentUserResolver = currentUserResolver;
        _logger = logger;
    }

    public async Task<WorkItemSummary> GetWorkItemAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
            throw new ArgumentOutOfRangeException(nameof(workItemId), "Work item id must be positive.");

        var url = BuildWorkItemUrl(workItemId);
        _logger.LogDebug("GET work item {WorkItemId} at {Url}", workItemId, url);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "GET work item {WorkItemId} failed with {StatusCode}. Body: {Body}",
                workItemId,
                response.StatusCode,
                LogFormatting.Truncate(body));
            throw new InvalidOperationException(
                $"Azure DevOps API error ({response.StatusCode}): {body}");
        }

        _logger.LogDebug("GET work item {WorkItemId} succeeded", workItemId);

        return ParseWorkItemSummary(body);
    }

    public async Task<int> CreateTaskAsync(
        string userText,
        double? completedWorkHours = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
            throw new ArgumentException("Work item text is required.", nameof(userText));

        _logger.LogInformation(
            "Creating {WorkItemType} (user text length {TextLength}, completedWork={CompletedWork})",
            _workItemOptions.WorkItemType,
            userText.Trim().Length,
            completedWorkHours);

        var fields = await BuildCreateFieldsAsync(userText, completedWorkHours, cancellationToken);
        _logger.LogDebug(
            "Create payload fields: {Fields}",
            LogFormatting.SummarizeWorkItemFields(fields));
        var patchDocument = fields.Select(pair => new
        {
            op = "add",
            path = $"/fields/{pair.Key}",
            value = pair.Value
        }).ToArray();

        var workItemType = Uri.EscapeDataString(_workItemOptions.WorkItemType);
        var url = $"{BuildProjectApiBase()}/_apis/wit/workitems/${workItemType}?api-version={_adoOptions.ApiVersion}";

        var patchJson = JsonSerializer.Serialize(patchDocument);
        _logger.LogDebug("POST create work item to {Url}. Patch: {Patch}", url, LogFormatting.Truncate(patchJson));

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(
            patchJson,
            Encoding.UTF8,
            "application/json-patch+json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Create work item failed with {StatusCode}. Body: {Body}",
                response.StatusCode,
                LogFormatting.Truncate(body));
            throw new InvalidOperationException(
                $"Azure DevOps API error ({response.StatusCode}): {body}");
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("id", out var idElement))
        {
            _logger.LogError(
                "Create work item response missing id. Body: {Body}",
                LogFormatting.Truncate(body));
            throw new InvalidOperationException("Azure DevOps response did not include work item id.");
        }

        var workItemId = idElement.GetInt32();
        _logger.LogInformation("Created work item {WorkItemId}", workItemId);
        return workItemId;
    }

    private async Task<Dictionary<string, object>> BuildCreateFieldsAsync(
        string userText,
        double? completedWorkHours,
        CancellationToken cancellationToken)
    {
        var fields = _workItemOptions.MandatoryFields.ToDictionary(
            pair => pair.Key,
            pair => (object)pair.Value);

        if (!fields.ContainsKey(WorkItemFields.StartDate))
        {
            fields[WorkItemFields.StartDate] = WorkItemFieldValues.TodayStartDate();
        }
        else
        {
            fields[WorkItemFields.StartDate] = WorkItemFieldValues.NormalizeStartDate(
                fields[WorkItemFields.StartDate]);
        }

        if (completedWorkHours.HasValue)
        {
            fields[WorkItemFields.CompletedWork] = completedWorkHours.Value;
        }
        else if (!fields.ContainsKey(WorkItemFields.CompletedWork))
        {
            fields[WorkItemFields.CompletedWork] =
                WorkItemFieldValues.ResolveDefaultCompletedWorkHours(_workItemOptions.MandatoryFields);
        }

        if (!fields.ContainsKey(WorkItemFields.AssignedTo))
        {
            fields[WorkItemFields.AssignedTo] =
                await _currentUserResolver.GetAssignedToValueAsync(cancellationToken);
        }
        else if (IsCurrentUserToken(fields[WorkItemFields.AssignedTo]))
        {
            fields[WorkItemFields.AssignedTo] =
                await _currentUserResolver.GetAssignedToValueAsync(cancellationToken);
        }

        if (fields.ContainsKey(WorkItemFields.Tags))
        {
            fields[WorkItemFields.Tags] = WorkItemFieldValues.NormalizeTags(
                fields[WorkItemFields.Tags]);
        }

        fields[_workItemOptions.UserTextField] = userText.Trim();
        return fields;
    }

    private static bool IsCurrentUserToken(object value)
    {
        return value is string text
            && string.Equals(text.Trim(), WorkItemFields.CurrentUserToken, StringComparison.OrdinalIgnoreCase);
    }

    private string BuildProjectApiBase()
    {
        var project = Uri.EscapeDataString(_adoOptions.Project);
        return $"{_adoOptions.ServerUrl.TrimEnd('/')}/{project}";
    }

    private string BuildWorkItemUrl(int workItemId)
    {
        return $"{BuildProjectApiBase()}/_apis/wit/workitems/{workItemId}?api-version={_adoOptions.ApiVersion}";
    }

    private static WorkItemSummary ParseWorkItemSummary(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("id", out var idElement))
            throw new InvalidOperationException("Azure DevOps response did not include work item id.");

        var summary = new WorkItemSummary { Id = idElement.GetInt32() };

        if (root.TryGetProperty("fields", out var fields))
        {
            summary.WorkItemType = GetFieldString(fields, "System.WorkItemType");
            summary.Title = GetFieldString(fields, "System.Title");
            summary.State = GetFieldString(fields, "System.State");
        }

        return summary;
    }

    private static string GetFieldString(JsonElement fields, string fieldName)
    {
        if (fields.TryGetProperty(fieldName, out var value))
            return value.GetString() ?? "";

        return "";
    }
}
