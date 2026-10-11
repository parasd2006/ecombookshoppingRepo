using ecom_bookshopping_1003.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ecom_bookshopping_1003.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IEmailSender _emailSender;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger,
        IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public bool ShowResendVerification { get; set; } = false;

    public class InputModel
    {
        [Required(ErrorMessage = "Email ya Phone Number enter karein")]
        [Display(Name = "Email ya Phone Number")]
        public string EmailOrPhone { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            var loginInput = Input.EmailOrPhone.Trim();
            ApplicationUser? user = null;

            if (loginInput.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginInput);
            }
            else
            {
                string withZero = loginInput.StartsWith("0") ? loginInput : "0" + loginInput;
                string withoutZero = loginInput.StartsWith("0") ? loginInput.TrimStart('0') : loginInput;

                user = await _userManager.Users.FirstOrDefaultAsync(u =>
                    u.PhoneNumber == loginInput ||
                    u.PhoneNumber == withZero ||
                    u.PhoneNumber == withoutZero);
            }

            if (user != null)
            {
                // 1. Password check karo
                var passwordCheck = await _userManager.CheckPasswordAsync(user, Input.Password);

                if (passwordCheck)
                {
                    // 2. Agar email verified nahi hai toh professional HTML email bhej do
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme)!;

                        // Attractive HTML template generate karna
                        string emailBody = BuildProfessionalEmailTemplate(callbackUrl, user.UserName ?? "Book Lover");

                        await _emailSender.SendEmailAsync(
                            user.Email!,
                            "Verify Your BookKart Account",
                            emailBody);

                        // Flag true set karo taaki view me password hide ho sake
                        ShowResendVerification = true;
                        ModelState.AddModelError(string.Empty, "Verification Link sent to your email. Please check your inbox...");
                        return Page();
                    }

                    // 3. Agar email confirmed hai, direct sign-in karo
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName!,
                        Input.Password,
                        Input.RememberMe,
                        lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Sahi Email/Phone aur password dalein.");
            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        // Resend karte waqt password required validation ko hata do
        ModelState.Remove("Input.Password");

        var loginInput = Input.EmailOrPhone?.Trim();
        ApplicationUser? user = null;

        if (!string.IsNullOrEmpty(loginInput))
        {
            if (loginInput.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginInput);
            }
            else
            {
                string withZero = loginInput.StartsWith("0") ? loginInput : "0" + loginInput;
                string withoutZero = loginInput.StartsWith("0") ? loginInput.TrimStart('0') : loginInput;

                user = await _userManager.Users.FirstOrDefaultAsync(u =>
                    u.PhoneNumber == loginInput ||
                    u.PhoneNumber == withZero ||
                    u.PhoneNumber == withoutZero);
            }
        }

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User account nahi mila.");
            return Page();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId = userId, code = code },
            protocol: Request.Scheme)!;

        // Attractive HTML template generate karna
        string emailBody = BuildProfessionalEmailTemplate(callbackUrl, user.UserName ?? "Book Lover");

        await _emailSender.SendEmailAsync(
            user.Email!,
            "Verify Your BookKart Account",
            emailBody);

        ModelState.AddModelError(string.Empty, "Verification Link sent to your email. Please check your inbox...");
        ShowResendVerification = true;
        return Page();
    }

    // Professional HTML Email Template Builder
    private static string BuildProfessionalEmailTemplate(string confirmationUrl, string recipientName)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <title>Verify Your Email - BookKart</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f1f5f9; font-family: Arial, sans-serif;'>
    <table width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #f1f5f9; padding: 30px 10px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellspacing='0' cellpadding='0' border='0' style='max-width: 540px; background-color: #ffffff; border-radius: 16px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 10px 25px rgba(0,0,0,0.08);'>
                    
                    <!-- 3D Header Section -->
                    <tr>
                        <td align='center' style='background-color: #0b1f3a; padding: 35px 20px;'>
                            <!-- 3D Animated Book Icon -->
                            <img src='https://cdn.dribbble.com/users/1299339/screenshots/3963434/book-flip.gif' 
                                 alt='BookKart' 
                                 width='90' 
                                 height='90' 
                                 style='border-radius: 14px; display: block; margin-bottom: 12px; box-shadow: 0 8px 16px rgba(0,0,0,0.3);' />
                            
                            <h1 style='color: #ffffff; margin: 0; font-size: 26px; font-weight: bold; letter-spacing: 0.5px;'>BookKart</h1>
                            <p style='color: #94a3b8; margin: 4px 0 0 0; font-size: 12px; letter-spacing: 1.5px; text-transform: uppercase;'>Online Bookstore</p>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 35px 30px;'>
                            <h2 style='color: #0b1f3a; font-size: 20px; margin-top: 0; margin-bottom: 12px;'>Confirm Your Email Address</h2>
                            <p style='font-size: 15px; line-height: 1.6; color: #475569; margin-bottom: 25px;'>
                                Hello <strong>{HtmlEncoder.Default.Encode(recipientName)}</strong>,<br><br>
                                Thank you for creating an account with BookKart! To activate your account and start exploring our book collection, best deals, and personal recommendations, please verify your email address.
                            </p>

                            <!-- 3D Effect CTA Button -->
                            <div style='text-align: center; margin: 35px 0;'>
                                <a href='{HtmlEncoder.Default.Encode(confirmationUrl)}' 
                                   style='background-color: #ff9f00; 
                                          color: #ffffff; 
                                          text-decoration: none; 
                                          padding: 15px 36px; 
                                          font-size: 16px; 
                                          font-weight: bold; 
                                          border-radius: 10px; 
                                          display: inline-block; 
                                          border-bottom: 4px solid #cc7000; 
                                          box-shadow: 0 6px 12px rgba(255, 159, 0, 0.4);'>
                                    Verify Email & Activate Account
                                </a>
                            </div>

                            <!-- Security Note -->
                            <div style='background-color: #f8fafc; border-left: 4px solid #2874f0; padding: 12px 14px; border-radius: 6px; margin-top: 25px;'>
                                <p style='font-size: 12px; color: #64748b; margin: 0;'>
                                    🔒 If you did not create this account, please ignore this email. No further action is required.
                                </p>
                            </div>
                        </td>
                    </tr>

                    <!-- Footer & Signature -->
                    <tr>
                        <td align='center' style='background-color: #f8fafc; padding: 22px 25px; border-top: 1px solid #e2e8f0;'>
                            <p style='font-size: 14px; font-weight: bold; color: #0b1f3a; margin: 0 0 5px 0;'>
                                Crafted &amp; Developed with ❤️ by Paras Dhiman
                            </p>
                            <p style='font-size: 12px; color: #94a3b8; margin: 0;'>
                                &copy; {DateTime.UtcNow.Year} BookKart. All rights reserved.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}