using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using StockSense.Infrastructure.Data;
using StockSense.Application.Interfaces;

namespace StockSense.Infrastructure.Services;

public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        // ... (Keep your existing HTML template) ...
        string htmlMessage = $@"<div... Confirm your email ...</div>";
        await SendEmailAsync(email, "StockSense - Confirm your email", htmlMessage);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        // ... (Keep your existing HTML template) ...
        string htmlMessage = $@"<div... Reset your password ...</div>";
        await SendEmailAsync(email, "StockSense - Reset your password", htmlMessage);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        // ... (Keep your existing HTML template) ...
        string htmlMessage = $@"<div... Your Reset Code ...</div>";
        await SendEmailAsync(email, "StockSense - Your reset code", htmlMessage);
    }

    // Keep this as a private helper method just for Identity emails
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("StockSense Support", _config["Smtp:User"] ?? "noreply@yourdomain.com"));
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
}