using Microsoft.Extensions.Options;
using SAProject.Models;
using SAProject.Services;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(
                from: _smtpSettings.SenderEmail,
                to: toEmail,
                subject: subject,
                body: body
            );
            mailMessage.IsBodyHtml = true;

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {toEmail}");
            throw;
        }
    }

    public async Task SendMfaTokenAsync(string toEmail, string token, string userName)
    {
        var subject = "🔐 Mã xác thực đăng nhập - Hệ thống Bảo mật";
        var body = BuildMfaEmailTemplate(token, userName);

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendSecurityAlertAsync(string toEmail, string action, string userName)
    {
        var subject = "🚨 Cảnh báo bảo mật - Hoạt động đáng chú ý";
        var body = BuildSecurityAlertTemplate(action, userName);

        await SendEmailAsync(toEmail, subject, body);
    }

    private string BuildMfaEmailTemplate(string token, string userName)
    {
        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Mã xác thực đăng nhập</title>
    <style>
        body {{
            font-family: 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background-color: #f6f9fc;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            border: 1px solid #e0e6ed;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-align: center;
            padding: 30px 20px;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
            font-weight: 600;
        }}
        .header p {{
            margin: 8px 0 0 0;
            opacity: 0.9;
            font-size: 16px;
        }}
        .content {{
            padding: 30px;
            line-height: 1.6;
        }}
        .token-container {{
            background-color: #f8f9fa;
            border: 2px dashed #dee2e6;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            margin: 25px 0;
        }}
        .token {{
            font-size: 32px;
            font-weight: bold;
            color: #2c3e50;
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
        }}
        .warning {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 6px;
            padding: 15px;
            margin: 20px 0;
            color: #856404;
        }}
        .info-box {{
            background-color: #e3f2fd;
            border-radius: 6px;
            padding: 15px;
            margin: 20px 0;
        }}
        .footer {{
            background-color: #f8f9fa;
            text-align: center;
            font-size: 14px;
            color: #6c757d;
            padding: 20px;
            border-top: 1px solid #e9ecef;
        }}
        .security-badge {{
            display: inline-block;
            background: #28a745;
            color: white;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
            margin-left: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Xác thực đa yếu tố</h1>
            <p>Hệ thống Bảo mật - SAProject</p>
        </div>
        
        <div class='content'>
            <p>Xin chào <strong>{WebUtility.HtmlEncode(userName)}</strong>,</p>
            
            <p>Chúng tôi đã nhận được yêu cầu đăng nhập vào tài khoản của bạn. Để hoàn tất quá trình xác thực, vui lòng sử dụng mã sau:</p>
            
            <div class='token-container'>
                <div class='token'>{token}</div>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>Mã xác thực có hiệu lực trong <strong>10 phút</strong></li>
                    <li>Không chia sẻ mã này với bất kỳ ai</li>
                    <li>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email</li>
                </ul>
            </div>
            
            <div class='info-box'>
                <strong>ℹ️ Thông tin bảo mật:</strong>
                <p>Đây là một phần của hệ thống xác thực đa yếu tố (MFA) để bảo vệ tài khoản của bạn khỏi truy cập trái phép.</p>
            </div>
            
            <p>Trân trọng,<br>
            <strong>Đội ngũ Bảo mật - SAProject</strong> <span class='security-badge'>BẢO MẬT</span></p>
        </div>
        
        <div class='footer'>
            <p>© 2025 SAProject - Hệ thống Quản lý Bảo mật. Mọi quyền được bảo lưu.</p>
            <p>Đây là email tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string BuildSecurityAlertTemplate(string action, string userName)
    {
        var actionText = action switch
        {
            "Login with MFA Success" => "đăng nhập thành công với xác thực đa yếu tố",
            "Login Success" => "đăng nhập thành công",
            "Password Changed" => "thay đổi mật khẩu",
            "MFA Enabled" => "bật xác thực đa yếu tố",
            "MFA Disabled" => "tắt xác thực đa yếu tố",
            _ => action
        };

        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Cảnh báo bảo mật</title>
    <style>
        body {{
            font-family: 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background-color: #f6f9fc;
            margin: 0;
            padding: 0;
            color: #333;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            border: 1px solid #e0e6ed;
        }}
        .header {{
            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%);
            color: white;
            text-align: center;
            padding: 25px 20px;
        }}
        .header h1 {{
            margin: 0;
            font-size: 26px;
            font-weight: 600;
        }}
        .alert-icon {{
            font-size: 48px;
            margin-bottom: 10px;
        }}
        .content {{
            padding: 30px;
            line-height: 1.6;
        }}
        .alert-box {{
            background-color: #ffeaa7;
            border: 2px solid #fdcb6e;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
            text-align: center;
        }}
        .info-box {{
            background-color: #dfe6e9;
            border-radius: 6px;
            padding: 15px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .footer {{
            background-color: #f8f9fa;
            text-align: center;
            font-size: 14px;
            color: #6c757d;
            padding: 20px;
            border-top: 1px solid #e9ecef;
        }}
        .action-button {{
            display: inline-block;
            background: #e74c3c;
            color: white;
            padding: 12px 24px;
            border-radius: 6px;
            text-decoration: none;
            font-weight: 600;
            margin: 10px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='alert-icon'>🚨</div>
            <h1>Cảnh báo bảo mật</h1>
            <p>Hoạt động đáng chú ý trên tài khoản của bạn</p>
        </div>
        
        <div class='content'>
            <p>Xin chào <strong>{WebUtility.HtmlEncode(userName)}</strong>,</p>
            
            <p>Hệ thống bảo mật của chúng tôi vừa ghi nhận một hoạt động trên tài khoản của bạn:</p>
            
            <div class='alert-box'>
                <strong>📋 Hoạt động:</strong> {actionText}<br>
                <strong>⏰ Thời gian:</strong> {DateTime.Now:HH:mm:ss dd/MM/yyyy}<br>
                <strong>🌐 Địa chỉ IP:</strong> Đang được ghi nhận
            </div>
            
            <div class='info-box'>
                <strong>Nếu đây là bạn:</strong>
                <p>Bạn không cần thực hiện hành động nào. Thông báo này giúp bạn theo dõi các hoạt động trên tài khoản.</p>
                
                <strong>Nếu đây không phải là bạn:</strong>
                <p>Vui lòng thay đổi mật khẩu ngay lập tức và liên hệ với bộ phận hỗ trợ.</p>
            </div>
            
            <p style='text-align: center;'>
                <a href='#' class='action-button'>Kiểm tra hoạt động</a>
            </p>
            
            <p>Trân trọng,<br>
            <strong>Đội ngũ An ninh - SAProject</strong></p>
        </div>
        
        <div class='footer'>
            <p>© 2025 SAProject - Hệ thống Giám sát Bảo mật</p>
            <p>Để bảo vệ tài khoản, vui lòng không chia sẻ thông tin đăng nhập của bạn.</p>
        </div>
    </div>
</body>
</html>";
    }
}