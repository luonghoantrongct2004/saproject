using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SAProject.Data;
using SAProject.Models;

namespace SAProject.Services
{
    public class MfaService : IMfaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MfaService(ApplicationDbContext context, IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(string userId)
        {
            // Xóa token cũ
            var oldTokens = _context.MfaTokens.Where(t => t.UserId == userId && !t.IsUsed);
            _context.MfaTokens.RemoveRange(oldTokens);

            // Tạo token mới (6 chữ số)
            var random = new Random();
            var token = random.Next(100000, 999999).ToString();

            var mfaToken = new MfaToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _context.MfaTokens.Add(mfaToken);
            await _context.SaveChangesAsync();

            // Gửi token qua email với template mới
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _emailService.SendMfaTokenAsync(user.Email, token, user.UserName);
            }

            return token;
        }

        public async Task<bool> ValidateTokenAsync(string userId, string token)
        {
            var mfaToken = await _context.MfaTokens
                .FirstOrDefaultAsync(t =>
                    t.UserId == userId &&
                    t.Token == token &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (mfaToken != null)
            {
                mfaToken.IsUsed = true;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> IsMfaRequiredAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.RequireMfa ?? false;
        }
    }
}
