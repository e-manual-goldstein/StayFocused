using System.Net.Http;
using System.Text.Json;
using DailyWorkLog.Options;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public class CurrentUserResolver
{
    private readonly HttpClient _httpClient;
    private readonly AzureDevOpsOptions _adoOptions;
    private string? _cachedAssignedTo;

    public CurrentUserResolver(
        HttpClient httpClient,
        IOptions<AzureDevOpsOptions> adoOptions)
    {
        _httpClient = httpClient;
        _adoOptions = adoOptions.Value;
    }

    public async Task<string> GetAssignedToValueAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_cachedAssignedTo))
            return _cachedAssignedTo;

        var connectionDataUrl =
            $"{BuildCollectionApiBase()}/_apis/connectionData?connectOptions=1&api-version={_adoOptions.ApiVersion}";

        using var connectionResponse = await _httpClient.GetAsync(connectionDataUrl, cancellationToken);
        var connectionBody = await connectionResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!connectionResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Could not resolve current user for System.AssignedTo ({connectionResponse.StatusCode}): {connectionBody}");
        }

        using var connectionDocument = JsonDocument.Parse(connectionBody);
        var root = connectionDocument.RootElement;

        if (!root.TryGetProperty("authenticatedUser", out var authenticatedUser))
        {
            throw new InvalidOperationException("Azure DevOps connectionData did not include authenticatedUser.");
        }

        var account = GetAccountProperty(authenticatedUser);
        if (string.IsNullOrWhiteSpace(account))
        {
            throw new InvalidOperationException(
                "Azure DevOps authenticatedUser did not include properties.Account.$value.");
        }

        _cachedAssignedTo = account;
        return _cachedAssignedTo;
    }

    private static string? GetAccountProperty(JsonElement identity)
    {
        if (!identity.TryGetProperty("properties", out var properties))
            return null;

        if (!properties.TryGetProperty("Account", out var accountProperty))
            return null;

        if (accountProperty.TryGetProperty("$value", out var valueElement)
            && valueElement.ValueKind == JsonValueKind.String)
        {
            return valueElement.GetString();
        }

        return accountProperty.ValueKind == JsonValueKind.String
            ? accountProperty.GetString()
            : null;
    }

    private string BuildCollectionApiBase()
    {
        return _adoOptions.ServerUrl.TrimEnd('/');
    }
}
