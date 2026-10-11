// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ecom_bookshopping_1003.DataAccess.Repository.IRepository;
using ecom_bookshopping_1003.Models;
using ecom_bookshopping_1002.Utility;

using ecom_bookshopping_1003.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace ecom_bookshopping_1003.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWorkRepository _unitofwork;
    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWorkRepository unitOfWork,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender)

    {
        _roleManager = roleManager;
        _unitofwork = unitOfWork;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        //ADD CUSTOM COLS

        [Required]
        public string Name { get; set; }
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        [Display(Name = "PostalCode")]
        public String PostalCode { get; set; }
        [Display(Name = "Phone NUmber")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [NotMapped]
        public string Role { get; set; }
        public IEnumerable<SelectListItem> Rolelist { get; set; }
        public IEnumerable<SelectListItem> CompanyList { get; set; }
    }


    public async Task OnGetAsync(string? returnUrl = null)
    {
        Input = new InputModel()
        {
            CompanyList = _unitofwork.company.GetAll().Select(cl => new SelectListItem()
            {
                Text = cl.Name,
                Value = cl.Id.ToString(),
            }),
            Rolelist = _roleManager.Roles.Where(r => r.Name != SD.Role_Individual).Select
            (r => r.Name).Select(rl => new SelectListItem()
            {
                Text = rl,
                Value = rl,
            })
        };

        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    //public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    //{
    //    returnUrl ??= Url.Content("~/");
    //    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    //    if (ModelState.IsValid)
    //    {
    //        var user = new ApplicationUser()
    //        {
    //            Name = Input.Name,
    //            UserName = Input.Email,
    //            Email = Input.Email,
    //            PhoneNumber = Input.PhoneNumber,
    //            StreetAddress = Input.StreetAddress,
    //            City = Input.City,
    //            State = Input.State,
    //            PostalCode = Input.PostalCode,
    //            CompanyId = Input.CompanyId,
    //            Role = Input.Role
    //        };

    //        await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
    //        await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
    //        var result = await _userManager.CreateAsync(user, Input.Password);

    //        if (result.Succeeded)
    //        {
    //            _logger.LogInformation("User created a new account with password.");

    //            if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
    //            {
    //                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
    //            }
    //            if (!await _roleManager.RoleExistsAsync(SD.Role_Employee))
    //            {
    //                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
    //            }
    //            if (!await _roleManager.RoleExistsAsync(SD.Role_Individual))
    //            {
    //                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Individual));
    //            }
    //            if (!await _roleManager.RoleExistsAsync(SD.Role_Company))
    //            {
    //                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
    //            }
    //            //ADD ADMIN ROLE TO USER
    //            // await _userManager.AddToRoleAsync(user, SD.Role_Admin);

    //            var userId = await _userManager.GetUserIdAsync(user);
    //            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    //            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    //            var callbackUrl = Url.Page(
    //                "/Account/ConfirmEmail",
    //                pageHandler: null,
    //                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
    //                protocol: Request.Scheme)!;

    //            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
    //                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
    //            if (Input.Role == null && Input.CompanyId == null)
    //            {
    //                await _userManager.AddToRoleAsync(user, SD.Role_Individual);
    //            }
    //            else
    //                if (Input.CompanyId > 0)

    //                {
    //                    await _userManager.AddToRoleAsync(user, SD.Role_Company);
    //                }
    //                else
    //                {
    //                    await _userManager.AddToRoleAsync(user, SD.Role_Employee);
    //                }


    //            if (_userManager.Options.SignIn.RequireConfirmedAccount)
    //            {
    //                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
    //            }
    //            else
    //            {
    //                if (Input.Role == null && Input.CompanyId == null)
    //                {
    //                    await _signInManager.SignInAsync(user, isPersistent: false);
    //                    return LocalRedirect(returnUrl);
    //                }
    //                else
    //                {
    //                    return RedirectToAction("Index", "User", new { Area = "Admin" });
    //                }
    //            }
    //        }
    //        foreach (var error in result.Errors)
    //        {
    //            ModelState.AddModelError(string.Empty, error.Description);
    //        }
    //    }

    //    // If we got this far, something failed, redisplay form
    //    return Page();
    //}
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser()
            {
                Name = Input.Name,
                UserName = Input.Email,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                StreetAddress = Input.StreetAddress,
                City = Input.City,
                State = Input.State,
                PostalCode = Input.PostalCode,
                CompanyId = Input.CompanyId,
                Role = Input.Role
            };

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Step 1: Enable 2FA for this user
                await _userManager.SetTwoFactorEnabledAsync(user, true);

                if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                }
                if (!await _roleManager.RoleExistsAsync(SD.Role_Employee))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                }
                if (!await _roleManager.RoleExistsAsync(SD.Role_Individual))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Individual));
                }
                if (!await _roleManager.RoleExistsAsync(SD.Role_Company))
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
                }
                //ADD ADMIN ROLE TO USER
              //  await _userManager.AddToRoleAsync(user, SD.Role_Admin);
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme)!;

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (Input.Role == null && Input.CompanyId == null)
                {
                    await _userManager.AddToRoleAsync(user, SD.Role_Individual);
                }
                else if (Input.CompanyId > 0)
                {
                    await _userManager.AddToRoleAsync(user, SD.Role_Company);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, SD.Role_Employee);
                }

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }
                else
                {
                    if (Input.Role == null && Input.CompanyId == null)
                    {
                        // Step 2: Send to Login page so 2FA email OTP triggers
                        return RedirectToPage("Login", new { returnUrl = returnUrl });
                    }
                    else
                    {
                        return RedirectToAction("Index", "User", new { Area = "Admin" });
                    }
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}

