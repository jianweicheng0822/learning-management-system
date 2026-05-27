using System.Security.Claims;
using LMS.Models;
using LMS.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

// Handles user authentication (login/register/logout) and profile management
public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<AccountController> logger) : Controller
{
    // Show login form; redirects to home if already authenticated
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // Validate credentials and sign in; supports return URL redirect
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            logger.LogWarning("Login failed — unknown email {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed — wrong password for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        logger.LogInformation("User {UserId} ({Email}) logged in", user.Id, user.Email);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    // Create a new user account, assign role (Student or Instructor), and auto sign-in
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Role != "Student" && model.Role != "Instructor")
        {
            ModelState.AddModelError("Role", "Invalid role selected.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Registration failed for {Email}: {Errors}",
                model.Email, string.Join("; ", result.Errors.Select(e => e.Description)));
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await userManager.AddToRoleAsync(user, model.Role);
        await signInManager.SignInAsync(user, isPersistent: true);

        logger.LogInformation("User {UserId} ({Email}) registered as {Role}", user.Id, user.Email, model.Role);

        TempData["Success"] = "Registration successful! Welcome to LMS.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await signInManager.SignOutAsync();
        logger.LogInformation("User {UserId} logged out", userId);
        return RedirectToAction("Index", "Home");
    }

    // Display the current user's profile details and roles
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var roles = await userManager.GetRolesAsync(user);
        var model = new ProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Roles = roles
        };

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var roles = await userManager.GetRolesAsync(user);
        var model = new ProfileViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Roles = roles
        };

        return View(model);
    }

    // Update the user's display name
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        user.FullName = model.FullName;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction("Profile");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
