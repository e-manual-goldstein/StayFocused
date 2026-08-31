using System.Net.Http;
using System.Text;
using System.Text.Json;
using DailyWorkLog.Models;
using DailyWorkLog.Options;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class WorkItemService : IWorkItemService
{
    private readonly HttpClient _httpClient;
    private readonly AzureDevOpsOptions _adoOptions;
    private readonly WorkItemOptions _workItemOptions;
    private string? _cachedAssignedTo;

    public WorkItemService(
        HttpClient httpClient,
        IOptions<AzureDevOpsOptions> adoOptions,
        IOptions<WorkItemOptions> workItemOptions)
    {
        _httpClient = httpClient;
        _adoOptions = adoOptions.Value;
        _workItemOptions = workItemOptions.Value;
    }

    public async Task<WorkItemSummary> GetWorkItemAsync(
        int workItemId,
        CancellationToken cancellationToken = default)
    {
        if (workItemId <= 0)
            throw new ArgumentOutOfRangeException(nameof(workItemId), "Work item id must be positive.");

        var url = BuildWorkItemUrl(workItemId);
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Azure DevOps API error ({response.StatusCode}): {body}");

        return ParseWorkItemSummary(body);
    }

    public async Task<int> CreateTaskAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
            throw new ArgumentException("Work item text is required.", nameof(userText));

        var fields = await BuildCreateFieldsAsync(userText, cancellationToken);
        var patchDocument = fields.Select(pair => new
        {
            op = "add",
            path = $"/fields/{pair.Key}",
            value = pair.Value
        }).ToArray();

        var workItemType = Uri.EscapeDataString(_workItemOptions.WorkItemType);
        var url = $"{BuildProjectApiBase()}/_apis/wit/workitems/${workItemType}?api-version={_adoOptions.ApiVersion}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(
            JsonSerializer.Serialize(patchDocument),
            Encoding.UTF8,
            "application/json-patch+json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Azure DevOps API error ({response.StatusCode}): {body}");

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("id", out var idElement))
            throw new InvalidOperationException("Azure DevOps response did not include work item id.");

        return idElement.GetInt32();
    }

    private async Task<Dictionary<string, object>> BuildCreateFieldsAsync(
        string userText,
        CancellationToken cancellationToken)
    {
        var fields = _workItemOptions.MandatoryFields.ToDictionary(
            pair => pair.Key,
            pair => (object)pair.Value);

        if (!fields.ContainsKey(WorkItemFields.StartDate))
        {
            fields[WorkItemFields.StartDate] = DateTime.Today.ToString("yyyy-MM-dd");
        }

        if (!fields.ContainsKey(WorkItemFields.CompletedWork))
        {
            fields[WorkItemFields.CompletedWork] = WorkItemFields.DefaultCompletedWorkHours;
        }

        if (!fields.ContainsKey(WorkItemFields.AssignedTo))
        {
            fields[WorkItemFields.AssignedTo] = await GetCurrentUserAssignedToAsync(cancellationToken);
        }
        else if (IsCurrentUserToken(fields[WorkItemFields.AssignedTo]))
        {
            fields[WorkItemFields.AssignedTo] = await GetCurrentUserAssignedToAsync(cancellationToken);
        }

        fields[_workItemOptions.UserTextField] = userText.Trim();
        return fields;
    }

    private async Task<string> GetCurrentUserAssignedToAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedAssignedTo))
            return _cachedAssignedTo;

        var url =
            $"{BuildCollectionApiBase()}/_apis/connectionData?connectOptions=1&api-version={_adoOptions.ApiVersion}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Could not resolve current user for System.AssignedTo ({response.StatusCode}): {body}");

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("authenticatedUser", out var user))
            throw new InvalidOperationException("Azure DevOps connectionData did not include authenticatedUser.");

        var displayName = user.TryGetProperty("providerDisplayName", out var displayElement)
            ? displayElement.GetString()
            : null;
        var uniqueName = user.TryGetProperty("uniqueName", out var uniqueElement)
            ? uniqueElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(uniqueName))
            throw new InvalidOperationException("Azure DevOps authenticatedUser did not include uniqueName.");

        _cachedAssignedTo = string.IsNullOrWhiteSpace(displayName)
            ? uniqueName
            : $"{displayName} <{uniqueName}>";

        return _cachedAssignedTo;
    }

    private static bool IsCurrentUserToken(object value)
    {
        return value is string text
            && string.Equals(text.Trim(), WorkItemFields.CurrentUserToken, StringComparison.OrdinalIgnoreCase);
    }

    private string BuildCollectionApiBase()
    {
        return _adoOptions.ServerUrl.TrimEnd('/');
    }

    private string BuildProjectApiBase()
    {
        var project = Uri.EscapeDataString(_adoOptions.Project);
        return $"{BuildCollectionApiBase()}/{project}";
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
