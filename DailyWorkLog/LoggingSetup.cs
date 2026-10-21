using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
namespace DailyWorkLog;

public static class LoggingSetup
{
    public static string LogDirectory { get; private set; } = "";

    public static void Configure(IConfiguration configuration)
    {
        LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DailyWorkLog",
            "logs");
        Directory.CreateDirectory(LogDirectory);

        var logFilePath = Path.Combine(LogDirectory, "dailyworklog-.log");

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information(
            "Logging initialized. Log directory: {LogDirectory}",
            LogDirectory);
    }

    public static void Shutdown()
    {
        Log.Information("Application shutting down");
        Log.CloseAndFlush();
    }
}
