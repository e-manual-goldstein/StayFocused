using DailyWorkLog.Options;
using Microsoft.Extensions.Options;

namespace DailyWorkLog.Services;

public static class ConfigurationValidator
{
    public static void Validate(
        IOptions<AzureDevOpsOptions> adoOptions,
        IOptions<WorkItemOptions> workItemOptions,
        IOptions<DailyPromptOptions> promptOptions)
    {
        var ado = adoOptions.Value;
        var workItem = workItemOptions.Value;
        var prompt = promptOptions.Value;

        if (string.IsNullOrWhiteSpace(ado.ServerUrl))
            throw new InvalidOperationException(
                "AzureDevOps:ServerUrl is required in appsettings.json (e.g. https://tfs.mycompany.com/tfs/DefaultCollection).");

        if (!Uri.TryCreate(ado.ServerUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException(
                $"AzureDevOps:ServerUrl is not a valid URL: {ado.ServerUrl}");

        if (string.IsNullOrWhiteSpace(ado.Project))
            throw new InvalidOperationException("AzureDevOps:Project is required in appsettings.json.");

        if (string.IsNullOrWhiteSpace(workItem.WorkItemType))
            throw new InvalidOperationException("WorkItem:WorkItemType is required in appsettings.json.");

        if (string.IsNullOrWhiteSpace(workItem.UserTextField))
            throw new InvalidOperationException("WorkItem:UserTextField is required in appsettings.json.");

        if (!TimeSpan.TryParse(prompt.PromptTime, out _))
            throw new InvalidOperationException(
                $"DailyPrompt:PromptTime must be a valid time (e.g. 17:00). Got: {prompt.PromptTime}");
    }
}
