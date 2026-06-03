namespace FoodDelivery.API.Services.Email;

public interface IEmailSender
{
    Task SendPasswordResetAsync(
        string toEmail,
        string recipientName,
        string resetUrl,
        CancellationToken cancellationToken = default);
}
