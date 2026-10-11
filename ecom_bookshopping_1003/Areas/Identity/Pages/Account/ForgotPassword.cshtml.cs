// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ecom_bookshopping_1003.Data;
using ecom_bookshopping_1003.Models;

namespace ecom_bookshopping_1003.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required(ErrorMessage = "Please enter your registered email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = default!;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Security practice: Don't reveal whether user exists or not
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // 🔥 Email parameter add kiya gaya hai
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, email = Input.Email },
                protocol: Request.Scheme)!;

            // Professional HTML template generate karna
            string emailBody = BuildPasswordResetEmailTemplate(callbackUrl, user.UserName ?? "Book Lover");

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Reset Your BookKart Password",
                emailBody);

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }

    // Professional Password Reset Email Template Builder
    private static string BuildPasswordResetEmailTemplate(string resetUrl, string recipientName)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <title>Reset Your Password - BookKart</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f1f5f9; font-family: Arial, sans-serif;'>
    <table width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #f1f5f9; padding: 30px 10px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellspacing='0' cellpadding='0' border='0' style='max-width: 540px; background-color: #ffffff; border-radius: 16px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 10px 25px rgba(0,0,0,0.08);'>
                    
                    <!-- Header Section -->
                    <tr>
                        <td align='center' style='background-color: #0b1f3a; padding: 35px 20px;'>
                            <div style='background-color: #2874f0; width: 56px; height: 56px; line-height: 56px; border-radius: 14px; color: #ffffff; font-size: 26px; margin-bottom: 12px; box-shadow: 0 4px 12px rgba(40, 116, 240, 0.4);'>
                                🔐
                            </div>
                            <h1 style='color: #ffffff; margin: 0; font-size: 26px; font-weight: bold; letter-spacing: 0.5px;'>BookKart</h1>
                            <p style='color: #94a3b8; margin: 4px 0 0 0; font-size: 12px; letter-spacing: 1.5px; text-transform: uppercase;'>Secure Account Recovery</p>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 35px 30px;'>
                            <h2 style='color: #0b1f3a; font-size: 20px; margin-top: 0; margin-bottom: 12px;'>Reset Your Password</h2>
                            <p style='font-size: 15px; line-height: 1.6; color: #475569; margin-bottom: 25px;'>
                                Hello <strong>{HtmlEncoder.Default.Encode(recipientName)}</strong>,<br><br>
                                We received a request to reset the password for your BookKart account. Click the button below to choose a new password.
                            </p>

                            <!-- 3D CTA Button -->
                            <div style='text-align: center; margin: 35px 0;'>
                                <a href='{HtmlEncoder.Default.Encode(resetUrl)}' 
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
                                    Reset Password
                                </a>
                            </div>

                            <!-- Security Warning -->
                            <div style='background-color: #f8fafc; border-left: 4px solid #2874f0; padding: 12px 14px; border-radius: 6px; margin-top: 25px;'>
                                <p style='font-size: 12px; color: #64748b; margin: 0;'>
                                    🔒 If you did not request a password reset, you can safely ignore this email. Your password will remain unchanged.
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