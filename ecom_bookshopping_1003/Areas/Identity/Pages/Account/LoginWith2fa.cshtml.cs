using System.ComponentModel.DataAnnotations;
using ecom_bookshopping_1003.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging; // 👈 Yeh top par add karna hai

namespace ecom_bookshopping_1003.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<LoginWith2faModel> _logger;

        public LoginWith2faModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
        public string MaskedEmail { get; set; } = default!;

        public class InputModel
        {
            [Required(ErrorMessage = "OTP daalna zaroori hai")]
            [StringLength(6, ErrorMessage = "6 digit ka code daalein", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "OTP Code")]
            public string TwoFactorCode { get; set; } = default!;

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
            MaskedEmail = MaskEmail(user.Email!);

            await SendOtpAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            MaskedEmail = MaskEmail(user.Email!);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl ??= Url.Content("~/");
            var code = Input.TwoFactorCode.Replace(" ", string.Empty).Trim();

            var result = await _signInManager.TwoFactorSignInAsync("Email", code, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with email 2fa.");
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "OTP galat hai ya expire ho gaya hai.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostResendAsync(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            await SendOtpAsync(user);

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
            MaskedEmail = MaskEmail(user.Email!);
            ViewData["ResendMessage"] = "Naya OTP email par bhej diya gaya hai.";

            return Page();
        }

        private async Task SendOtpAsync(ApplicationUser user)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            await _emailSender.SendEmailAsync(user.Email!, "Aapka Login OTP - EcomBookShopping",
                $@"<div style='font-family:Arial;padding:20px'>
                     <h2>Login Verification</h2>
                     <p>Aapka OTP code hai:</p>
                     <p style='font-size:32px;font-weight:bold;letter-spacing:6px'>{code}</p>
                     <p>Ye code 5 minute me expire ho jayega.</p>
                   </div>");
        }

        private static string MaskEmail(string email)
        {
            var parts = email.Split('@');
            var name = parts[0];
            return name.Length <= 2
                ? $"{name[0]}***@{parts[1]}"
                : $"{name[..2]}{new string('*', name.Length - 2)}@{parts[1]}";
        }
    }
}