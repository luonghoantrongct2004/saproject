using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SAProject.Data;
using SAProject.Models;
using SAProject.Services;

namespace SAProject.Controllers
{
    public class MfaController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMfaService _mfaService;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;

        public MfaController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMfaService mfaService,
            IAuditService auditService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mfaService = mfaService;
            _auditService = auditService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> LoginWithMfa()
        {
            // Lấy user từ session
            var userId = HttpContext.Session.GetString("MfaUserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Phiên xác thực đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction("Login", "Account");
            }

            ViewData["ReturnUrl"] = HttpContext.Session.GetString("MfaReturnUrl");
            ViewData["RememberMe"] = bool.Parse(HttpContext.Session.GetString("MfaRememberMe") ?? "false");

            if (TempData["MfaReason"] != null)
            {
                ViewData["MfaReason"] = TempData["MfaReason"];
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithMfa(string token)
        {
            // Lấy user từ session
            var userId = HttpContext.Session.GetString("MfaUserId");
            var rememberMe = bool.Parse(HttpContext.Session.GetString("MfaRememberMe") ?? "false");
            var returnUrl = HttpContext.Session.GetString("MfaReturnUrl");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Phiên xác thực đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra token
            if (await _mfaService.ValidateTokenAsync(user.Id, token))
            {
                // Xóa session MFA
                HttpContext.Session.Remove("MfaUserId");
                HttpContext.Session.Remove("MfaRememberMe");
                HttpContext.Session.Remove("MfaReturnUrl");

                // RESET failed login count khi MFA thành công
                await ResetFailedLoginCountAsync(user);

                // Đăng nhập thành công
                await _signInManager.SignInAsync(user, isPersistent: rememberMe);

                // Audit log
                await _auditService.LogAsync(
                    userId: user.Id,
                    userName: user.UserName,
                    action: "Login with MFA Success",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                // Gửi cảnh báo bảo mật
                await _emailService.SendSecurityAlertAsync(user.Email, "Login with MFA Success", user.UserName);

                return Redirect("/");
            }
            else
            {
                // Token không hợp lệ
                await IncrementMfaFailedAttemptAsync(user);

                // Audit log cho failed MFA
                await _auditService.LogAsync(
                    userId: user.Id,
                    userName: user.UserName,
                    action: "MFA Failed - Invalid Token",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                ModelState.AddModelError(string.Empty, "Mã xác thực không hợp lệ hoặc đã hết hạn");

                // Giữ lại thông tin
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["RememberMe"] = rememberMe;

                return View();
            }
        }
        private async Task IncrementMfaFailedAttemptAsync(ApplicationUser user)
        {
            if (user != null)
            {
                user.LastFailedLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
        }
        private async Task ResetFailedLoginCountAsync(ApplicationUser user)
        {
            if (user != null && user.FailedLoginCount > 0)
            {
                user.FailedLoginCount = 0;
                user.LastFailedLoginDate = null;
                await _userManager.UpdateAsync(user);
            }
        }
    }

}