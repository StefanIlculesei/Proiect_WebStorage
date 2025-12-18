using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LoggingLayer;

/// <summary>
/// Service for sending error notification emails
/// </summary>
public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Sends an error notification email to the developer
    /// </summary>
    public async Task SendErrorNotificationAsync(string methodName, Exception exception, string? additionalContext = null)
    {
        try
        {
            var devEmail = _configuration["DevEmail"];
            if (string.IsNullOrEmpty(devEmail))
            {
                _logger.LogWarning("DevEmail not configured in settings. Error email notification skipped.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Smtp:FromName"] ?? "WebStorage",
                _configuration["Smtp:FromEmail"] ?? _configuration["Smtp:Username"]
            ));
            message.To.Add(new MailboxAddress("Developer", devEmail));
            message.Subject = $"[ERROR] {methodName} - {exception.GetType().Name}";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = BuildErrorEmailBody(methodName, exception, additionalContext),
                HtmlBody = BuildErrorEmailBodyHtml(methodName, exception, additionalContext)
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Smtp:Host"],
                int.Parse(_configuration["Smtp:Port"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await client.AuthenticateAsync(
                _configuration["Smtp:Username"],
                _configuration["Smtp:Password"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Error notification email sent to {devEmail}");
        }
        catch (Exception ex)
        {
            // Don't throw - we don't want email failures to break the application
            _logger.LogWarning($"Failed to send error notification email: {ex.Message}");
        }
    }

    private string BuildErrorEmailBody(string methodName, Exception exception, string? additionalContext)
    {
        var body = $@"An error occurred in the WebStorage API

Method: {methodName}
Exception Type: {exception.GetType().FullName}
Message: {exception.Message}

Stack Trace:
{exception.StackTrace}";

        if (!string.IsNullOrEmpty(additionalContext))
        {
            body += $@"

Additional Context:
{additionalContext}";
        }

        if (exception.InnerException != null)
        {
            body += $@"

Inner Exception: {exception.InnerException.GetType().FullName}
Inner Message: {exception.InnerException.Message}
Inner Stack Trace:
{exception.InnerException.StackTrace}";
        }

        body += $@"

Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        return body;
    }

    private string BuildErrorEmailBodyHtml(string methodName, Exception exception, string? additionalContext)
    {
        var contextHtml = !string.IsNullOrEmpty(additionalContext)
            ? $@"<tr><td style='padding: 8px; background-color: #f8f9fa; font-weight: bold;'>Additional Context:</td><td style='padding: 8px; background-color: #f8f9fa;'>{System.Net.WebUtility.HtmlEncode(additionalContext)}</td></tr>"
            : "";

        var innerExceptionHtml = exception.InnerException != null
            ? $@"<tr><td colspan='2' style='padding: 12px; background-color: #fff3cd; border-top: 2px solid #ffc107;'><strong>Inner Exception:</strong> {System.Net.WebUtility.HtmlEncode(exception.InnerException.GetType().FullName)}</td></tr>
                <tr><td style='padding: 8px; font-weight: bold;'>Inner Message:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(exception.InnerException.Message)}</td></tr>
                <tr><td style='padding: 8px; font-weight: bold;'>Inner Stack Trace:</td><td style='padding: 8px;'><pre style='margin: 0; font-size: 11px; overflow-x: auto;'>{System.Net.WebUtility.HtmlEncode(exception.InnerException.StackTrace ?? "N/A")}</pre></td></tr>"
            : "";

        return $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        h1 {{ color: #dc3545; border-bottom: 2px solid #dc3545; padding-bottom: 10px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        td {{ padding: 8px; border-bottom: 1px solid #dee2e6; vertical-align: top; }}
        pre {{ background-color: #f8f9fa; padding: 10px; border-radius: 4px; overflow-x: auto; }}
        .timestamp {{ text-align: center; margin-top: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>⚠️ WebStorage API Error</h1>
        <table>
            <tr><td style='padding: 8px; font-weight: bold; width: 180px;'>Method:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(methodName)}</td></tr>
            <tr><td style='padding: 8px; font-weight: bold;'>Exception Type:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(exception.GetType().FullName ?? "")}</td></tr>
            <tr><td style='padding: 8px; font-weight: bold;'>Message:</td><td style='padding: 8px; color: #dc3545; font-weight: bold;'>{System.Net.WebUtility.HtmlEncode(exception.Message)}</td></tr>
            {contextHtml}
            <tr><td style='padding: 8px; font-weight: bold;'>Stack Trace:</td><td style='padding: 8px;'><pre style='margin: 0; font-size: 11px;'>{System.Net.WebUtility.HtmlEncode(exception.StackTrace ?? "N/A")}</pre></td></tr>
            {innerExceptionHtml}
        </table>
        <div class='timestamp'>Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</div>
    </div>
</body>
</html>";
    }
}
