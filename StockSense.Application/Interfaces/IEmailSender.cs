using Microsoft.AspNetCore.Identity;

namespace StockSense.Application.Interfaces;

public interface IEmailSender<TUser> where TUser : class
{
    Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink);
    Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink);
    Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode);
    
    // Your custom general-purpose methods
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachment, string fileName);
}