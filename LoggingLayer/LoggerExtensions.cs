using Microsoft.Extensions.Logging;

namespace LoggingLayer;

/// <summary>
/// Extension methods for ILogger to simplify error logging
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs an error with method context information
    /// </summary>
    public static void LogError(this ILogger logger, string methodName, Exception exception, string? additionalContext = null)
    {
        var message = $"Error in {methodName}";
        if (!string.IsNullOrEmpty(additionalContext))
        {
            message += $": {additionalContext}";
        }

        logger.LogError(exception, message);
    }

    /// <summary>
    /// Logs a warning with method context information
    /// </summary>
    public static void LogWarning(this ILogger logger, string methodName, string message)
    {
        logger.LogWarning($"Warning in {methodName}: {message}");
    }

    /// <summary>
    /// Logs information with method context
    /// </summary>
    public static void LogInfo(this ILogger logger, string methodName, string message)
    {
        logger.LogInformation($"Info in {methodName}: {message}");
    }
}
