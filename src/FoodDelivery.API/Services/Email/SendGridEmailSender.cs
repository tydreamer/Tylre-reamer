using FoodDelivery.API.Options;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FoodDelivery.API.Services.Email;

public class SendGridEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SendGridEmailSender> logger) : IEmailSender
{
    public async Task SendPasswordResetAsync(
        string toEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Email disabled; password reset email not sent to {Email}", toEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            logger.LogWarning("SendGrid ApiKey missing; password reset email not sent to {Email}", toEmail);
            return;
        }

        var displayName = string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName;
        var htmlBody = $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(displayName)},</p>
            <p>We received a request to reset your password. Click the link below to choose a new password. This link expires in 1 hour.</p>
            <p><a href="{System.Net.WebUtility.HtmlEncode(resetUrl)}">Reset password</a></p>
            <p>If you did not request this, you can ignore this email.</p>
            """;

        var plainText = $"Reset your password: {resetUrl}";

        var client = new SendGridClient(settings.ApiKey);
        var from = new EmailAddress(settings.FromAddress, settings.FromName);
        var to = new EmailAddress(toEmail);
        var message = MailHelper.CreateSingleEmail(
            from,
            to,
            "Reset your Food Delivery password",
            plainText,
            htmlBody);

        var response = await client.SendEmailAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                "SendGrid failed for password reset to {Email}: {StatusCode} {Body}",
                toEmail,
                response.StatusCode,
                body);
            throw new InvalidOperationException($"SendGrid returned {(int)response.StatusCode}.");
        }

        logger.LogInformation("Password reset email sent to {Email} via SendGrid", toEmail);
    }
}
