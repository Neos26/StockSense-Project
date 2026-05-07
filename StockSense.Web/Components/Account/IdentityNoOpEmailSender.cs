using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using StockSense.Domain.Entities;

namespace StockSense.Web.Components.Account
{
    // Note: Once you are ready for production, you will replace the inner "NoOpEmailSender" 
    // with a real service like MailKit or SendGrid to actually dispatch these over the internet!
    internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
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
                            <a href='{confirmationLink}' style='display: inline-block; width: 100%; padding: 12px 0; border-radius: 6px; background-color: #18181b; color: #fafafa; font-size: 14px; font-weight: 500; text-decoration: none; box-sizing: border-box;'>
                                Confirm Account
                            </a>
                        </div>
                    </div>
                </div>";

            return emailSender.SendEmailAsync(email, "StockSense - Confirm your email", htmlMessage);
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
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
                            <a href='{resetLink}' style='display: inline-block; width: 100%; padding: 12px 0; border-radius: 6px; background-color: #18181b; color: #fafafa; font-size: 14px; font-weight: 500; text-decoration: none; box-sizing: border-box;'>
                                Reset Password
                            </a>
                        </div>
                        
                        <p style='text-align: center; font-size: 12px; color: #a1a1aa; margin-top: 24px;'>
                            If you didn't request a password reset, you can safely ignore this email.
                        </p>
                    </div>
                </div>";

            return emailSender.SendEmailAsync(email, "StockSense - Reset your password", htmlMessage);
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
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
                        
                        <div style='text-align: center; background-color: #f4f4f5; padding: 16px; border-radius: 6px; border: 1px dashed #e4e4e7; margin-bottom: 24px;'>
                            <span style='font-size: 28px; font-weight: 700; color: #09090b; letter-spacing: 4px;'>{resetCode}</span>
                        </div>
                    </div>
                </div>";

            return emailSender.SendEmailAsync(email, "StockSense - Your password reset code", htmlMessage);
        }
    }
}