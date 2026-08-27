namespace DailyWorkLog.Options;

public class AzureDevOpsOptions
{
    public const string SectionName = "AzureDevOps";

    /// <summary>
    /// Collection base URL, e.g. https://tfs.mycompany.com/tfs/DefaultCollection
    /// </summary>
    public string ServerUrl { get; set; } = "";

    public string Project { get; set; } = "";
    public string ApiVersion { get; set; } = "7.0";
}
