using Serilog;
using Serilog.Core;
using Serilog.Events;
using Microsoft.Extensions.Logging;

namespace LoggingLayer;

/// <summary>
/// Configures and provides Serilog logger instances for the application
/// </summary>
public static class LoggerConfiguration
{
    private static readonly string LogsFolder = Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "..",
        "storage", "logs"
    );

    /// <summary>
    /// Configures Serilog with file and console sinks
    /// Logs are stored in a solution-level 'storage/logs' folder with daily files named webstorage_YYYY-MM-DD.log
    /// Log level is read from WEBSTORAGE_LOG_LEVEL environment variable (default: Error)
    /// </summary>
    public static Logger ConfigureLogger()
    {
        // Create logs directory if it doesn't exist
        if (!Directory.Exists(LogsFolder))
        {
            Directory.CreateDirectory(LogsFolder);
        }

        // Get log level from environment variable or default to Error
        var logLevelString = Environment.GetEnvironmentVariable("WEBSTORAGE_LOG_LEVEL") ?? "Error";
        var logLevel = Enum.TryParse<LogEventLevel>(logLevelString, ignoreCase: true, out var level)
            ? level
            : LogEventLevel.Error;

        var logPath = LogsFolder + Path.DirectorySeparatorChar + "webstorage_.log";

        var logger = new Serilog.LoggerConfiguration()
            .MinimumLevel.ControlledBy(new LoggingLevelSwitch(logLevel))
            .WriteTo.File(
                logPath,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: null,
                retainedFileCountLimit: null
            )
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            )
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "WebStorage")
            .CreateLogger();

        return logger;
    }

    /// <summary>
    /// Configures Serilog in the ILoggingBuilder for dependency injection
    /// </summary>
    public static ILoggingBuilder AddSerilogLogging(this ILoggingBuilder builder)
    {
        var logger = ConfigureLogger();
        builder.AddSerilog(logger);
        return builder;
    }
}
