using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using StockSense.Data;

public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        string htmlMessage = $@"
            <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f4f4f5; padding: 40px 20px;'>
                <div style='max-width: 450px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e4e4e7; border-radius: 8px; padding: 32px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);'>
                    <h1 style='text-align: center; font-size: 24px; font-weight: 600; color: #09090b; margin: 0;'>Confirm your email</h1>
                    <h2 style='text-align: center; font-size: 16px; font-weight: 500; color: #71717a; margin-top: 8px; margin-bottom: 24px;'>Welcome to StockSense!</h2>
                    
                    <div style='border-top: 1px solid #e4e4e7; margin: 24px 0;'></div>
                    
                    <p style='text-align: center; font-size: 14px; color: #71717a; margin-bottom: 24px;'>
                        Please confirm your account registration by clicking the button below.
                    </p>
                    
                    <div style='text-align: center;'>
                        <a href='{confirmationLink}' style='display: inline-block; width: 100%; padding: 12px 0; border-radius: 6px; background-color: #dc2626; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; box-sizing: border-box; transition: background-color 0.2s;'>
                            Confirm Account
                        </a>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(email, "StockSense - Confirm your email", htmlMessage);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        string htmlMessage = $@"
            <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f4f4f5; padding: 40px 20px;'>
                <div style='max-width: 450px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e4e4e7; border-radius: 8px; padding: 32px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);'>
                    <h1 style='text-align: center; font-size: 24px; font-weight: 600; color: #09090b; margin: 0;'>Reset your password</h1>
                    <h2 style='text-align: center; font-size: 16px; font-weight: 500; color: #71717a; margin-top: 8px; margin-bottom: 24px;'>StockSense Account Recovery</h2>
                    
                    <div style='border-top: 1px solid #e4e4e7; margin: 24px 0;'></div>
                    
                    <p style='text-align: center; font-size: 14px; color: #71717a; margin-bottom: 24px;'>
                        We received a request to reset the password for your account. Click the button below to choose a new password.
                    </p>
                    
                    <div style='text-align: center;'>
                        <a href='{resetLink}' style='display: inline-block; width: 100%; padding: 12px 0; border-radius: 6px; background-color: #dc2626; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; box-sizing: border-box;'>
                            Reset Password
                        </a>
                    </div>
                    
                    <p style='text-align: center; font-size: 12px; color: #a1a1aa; margin-top: 24px;'>
                        If you didn't request a password reset, you can safely ignore this email.
                    </p>
                </div>
            </div>";

        await SendEmailAsync(email, "StockSense - Reset your password", htmlMessage);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        string htmlMessage = $@"
            <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: #f4f4f5; padding: 40px 20px;'>
                <div style='max-width: 450px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e4e4e7; border-radius: 8px; padding: 32px; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);'>
                    <h1 style='text-align: center; font-size: 24px; font-weight: 600; color: #09090b; margin: 0;'>Your Reset Code</h1>
                    <h2 style='text-align: center; font-size: 16px; font-weight: 500; color: #71717a; margin-top: 8px; margin-bottom: 24px;'>StockSense Account Recovery</h2>
                    
                    <div style='border-top: 1px solid #e4e4e7; margin: 24px 0;'></div>
                    
                    <p style='text-align: center; font-size: 14px; color: #71717a; margin-bottom: 16px;'>
                        Please use the following code to reset your password:
                    </p>
                    
                    <div style='text-align: center; background-color: #fef2f2; padding: 16px; border-radius: 6px; border: 1px dashed #fca5a5; margin-bottom: 24px;'>
                        <span style='font-size: 28px; font-weight: 700; color: #dc2626; letter-spacing: 4px;'>{resetCode}</span>
                    </div>
                </div>
            </div>";

        await SendEmailAsync(email, "StockSense - Your reset code", htmlMessage);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("StockSense Support", "noreply@yourdomain.com")); // Consider pulling this from _config too!
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        int port = _config.GetValue<int>("Smtp:Port");

        await client.ConnectAsync(_config["Smtp:Host"], port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_config["Smtp:User"], _config["Smtp:Pass"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachment, string fileName)
    {
        var smtpUser = _config["Smtp:User"];
        if (string.IsNullOrEmpty(smtpUser))
            throw new InvalidOperationException("SMTP user is not configured.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("StockSense Support", smtpUser));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };

        if (attachment != null)
        {
            builder.Attachments.Add(fileName, attachment);
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        int port = _config.GetValue<int>("Smtp:Port");

        await client.ConnectAsync(_config["Smtp:Host"], port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtpUser, _config["Smtp:Pass"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}