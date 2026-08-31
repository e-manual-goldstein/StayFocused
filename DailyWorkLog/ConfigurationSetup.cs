using Microsoft.Extensions.Configuration;

namespace DailyWorkLog;

public static class ConfigurationSetup
{
    public const string BaseSettingsFileName = "appsettings.json";
    public const string LocalSettingsFileName = "appsettings.Local.json";

    public static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(BaseSettingsFileName, optional: false, reloadOnChange: true)
            .AddJsonFile(LocalSettingsFileName, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
