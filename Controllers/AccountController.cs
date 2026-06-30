using Microsoft.AspNetCore.Mvc;
using HonoursProject.Services;
using HonoursProject.Models;
using System.Security.Cryptography;

public class AccountController : Controller
{
    private readonly UserService _userService;
    public AccountController(UserService userService)
    {
        _userService = userService;
    }

    public IActionResult LoginPage()
    {
        return View();
    }

   [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _userService.GetUserByUsername(username);

        if (user == null)
        {
            ViewBag.Error = "Invalid username or password";
            return View("LoginPage");
        }

        // BCrypt verification
        if (!_userService.VerifyPassword(password, user.Password))
        {
            ViewBag.Error = "Invalid username or password";
            return View("LoginPage");
        }

        // Login success
        HttpContext.Session.SetString("username", user.Username);
        HttpContext.Session.SetString("profile_picture", user.ProfilePicture);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    public IActionResult ResetConfirmation()
    {
        return View();
    }

    public IActionResult ProfilePage()
    {
        var username = HttpContext.Session.GetString("username");

        if (username == null)
        {
            return RedirectToAction("LoginPage");
        }

        var user = _userService.GetUserByUsername(username);
        return View(user);
    }



    public IActionResult AchievementsPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        return View();
    }

    public IActionResult SettingsPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        return View();
    }

    public IActionResult EditProfilePage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        return View();
    }
    public IActionResult ChangePasswordPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }
        
        return View();
    }

    [HttpPost]
    public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage");

        if (NewPassword != ConfirmPassword)
        {
            TempData["PasswordMessage"] = "New passwords do not match.";
            return RedirectToAction("ChangePasswordPage");
        }

        var user = _userService.GetUserByUsername(username);

        if (!_userService.VerifyPassword(CurrentPassword, user.Password))
        {
            TempData["PasswordMessage"] = "Current password is incorrect.";
            return RedirectToAction("ChangePasswordPage");
        }

        var newHash = _userService.HashPassword(NewPassword);
        _userService.UpdatePassword(username, newHash);

        TempData["PasswordMessage"] = "Password updated successfully.";
        return RedirectToAction("ChangePasswordPage");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("LoginPage");
    }
}
