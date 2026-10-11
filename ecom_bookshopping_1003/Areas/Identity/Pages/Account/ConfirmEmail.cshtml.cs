// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using ecom_bookshopping_1003.Data;
using ecom_bookshopping_1003.Models;

namespace ecom_bookshopping_1003.Areas.Identity.Pages.Account;

public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager; // 🔥 1. SignInManager add kiya

    public ConfirmEmailModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager) // 🔥 2. Inject karwaya
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string code, string? returnUrl = null)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            // 🔥 3. Email confirm hote hi user ko turant auto-login karwaya
            await _signInManager.SignInAsync(user, isPersistent: false);

            // 🔥 4. Seedha website ke home page par bhej diya
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return Redirect("~/");
        }

        StatusMessage = "Error confirming your email.";
        return Page();
    }
}