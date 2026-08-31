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

        var account = GetIdentityProperty(authenticatedUser, "Account");
        if (string.IsNullOrWhiteSpace(account))
        {
            throw new InvalidOperationException(
                "Azure DevOps authenticatedUser did not include properties.Account.$value.");
        }

        var displayName = GetString(authenticatedUser, "providerDisplayName")
            ?? GetString(authenticatedUser, "customDisplayName");

        var identityName = account;
        if (TryGetIdentityId(authenticatedUser, out var identityId))
        {
            identityName = await ResolveIdentityNameAsync(identityId, account, cancellationToken);
        }
        else if (!account.Contains('\\', StringComparison.Ordinal))
        {
            var domain = GetIdentityProperty(authenticatedUser, "Domain");
            if (!string.IsNullOrWhiteSpace(domain))
                identityName = $"{domain}\\{account}";
        }

        _cachedAssignedTo = FormatAssignedTo(displayName, identityName);
        return _cachedAssignedTo;
    }

    private async Task<string> ResolveIdentityNameAsync(
        Guid identityId,
        string account,
        CancellationToken cancellationToken)
    {
        var identityUrl =
            $"{BuildCollectionApiBase()}/_apis/identities/{identityId:D}?api-version={_adoOptions.ApiVersion}";

        using var identityResponse = await _httpClient.GetAsync(identityUrl, cancellationToken);
        var identityBody = await identityResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!identityResponse.IsSuccessStatusCode)
            return account;

        using var identityDocument = JsonDocument.Parse(identityBody);
        var identity = identityDocument.RootElement;

        var uniqueName = GetString(identity, "uniqueName");
        if (!string.IsNullOrWhiteSpace(uniqueName))
            return uniqueName;

        var identityAccount = GetIdentityProperty(identity, "Account") ?? account;
        var domain = GetIdentityProperty(identity, "Domain");
        if (!string.IsNullOrWhiteSpace(domain) && !identityAccount.Contains('\\', StringComparison.Ordinal))
            return $"{domain}\\{identityAccount}";

        return identityAccount;
    }

    private static string FormatAssignedTo(string? displayName, string identityName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return identityName;

        return $"{displayName} <{identityName}>";
    }

    private static bool TryGetIdentityId(JsonElement identity, out Guid identityId)
    {
        identityId = default;
        if (!identity.TryGetProperty("id", out var idElement))
            return false;

        return idElement.ValueKind == JsonValueKind.String
            && Guid.TryParse(idElement.GetString(), out identityId);
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return null;

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? GetIdentityProperty(JsonElement identity, string propertyName)
    {
        if (!identity.TryGetProperty("properties", out var properties))
            return null;

        if (!properties.TryGetProperty(propertyName, out var property))
            return null;

        if (property.TryGetProperty("$value", out var valueElement)
            && valueElement.ValueKind == JsonValueKind.String)
        {
            return valueElement.GetString();
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private string BuildCollectionApiBase()
    {
        return _adoOptions.ServerUrl.TrimEnd('/');
    }
}
