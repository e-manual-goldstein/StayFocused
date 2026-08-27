using System.Net.Http;
using System.Text;
using System.Text.Json;
using DailyWorkLog.Options;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class AzureDevOpsWorkItemService : IAzureDevOpsWorkItemService
{
    private readonly HttpClient _httpClient;
    private readonly AzureDevOpsOptions _adoOptions;
    private readonly WorkItemOptions _workItemOptions;

    public AzureDevOpsWorkItemService(
        HttpClient httpClient,
        IOptions<AzureDevOpsOptions> adoOptions,
        IOptions<WorkItemOptions> workItemOptions)
    {
        _httpClient = httpClient;
        _adoOptions = adoOptions.Value;
        _workItemOptions = workItemOptions.Value;
    }

    public async Task<int> CreateTaskAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
            throw new ArgumentException("Work item text is required.", nameof(userText));

        var fields = new Dictionary<string, string>(_workItemOptions.MandatoryFields);
        fields[_workItemOptions.UserTextField] = userText.Trim();

        var patchDocument = fields.Select(pair => new
        {
            op = "add",
            path = $"/fields/{pair.Key}",
            value = pair.Value
        }).ToArray();

        var project = Uri.EscapeDataString(_adoOptions.Project);
        var workItemType = Uri.EscapeDataString(_workItemOptions.WorkItemType);
        var serverBase = _adoOptions.ServerUrl.TrimEnd('/');
        var url =
            $"{serverBase}/{project}/_apis/wit/workitems/${workItemType}?api-version={_adoOptions.ApiVersion}";

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
}
