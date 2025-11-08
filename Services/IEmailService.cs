namespace SAProject.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendMfaTokenAsync(string toEmail, string token, string userName);
        Task SendSecurityAlertAsync(string toEmail, string action, string userName);
    }
}
