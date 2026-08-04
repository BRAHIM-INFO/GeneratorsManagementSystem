using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IAuditService _auditService; // 🆕

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            IAuditService auditService, // 🆕
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService; // 🆕
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                await _auditService.LogLoginAsync(model.Email, true);

                if (user != null)
                {
                    user.LastLoginAt = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("تسجيل دخول المستخدم: {Email}", model.Email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                await _auditService.LogLoginAsync(model.Email, false, "الحساب مقفل. يرجى المحاولة لاحقاً.");
                ModelState.AddModelError("", "الحساب مقفل. يرجى المحاولة لاحقاً.");
            }
            else
            {
                await _auditService.LogLoginAsync(model.Email, false, "البريد الإلكتروني أو كلمة المرور غير صحيحة"); 
                ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await _auditService.LogLogoutAsync();
            return RedirectToAction("Welcome", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    public class LoginViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}