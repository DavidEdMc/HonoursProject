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

        string hashedInput = ComputeSha256(password);

        if (hashedInput != user.Password)
        {
            ViewBag.Error = "Invalid username or password";
            return View("LoginPage");
        }

        // Login success
        HttpContext.Session.SetString("username", user.Username);
        HttpContext.Session.SetString("profile_picture", user.ProfilePicture);

        return RedirectToAction("Index", "Home");
    }

    private string ComputeSha256(string raw)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
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
        return View();
    }

    public IActionResult SettingsPage()
    {
        return View();
    }

    public IActionResult EditProfilePage()
    {
        return View();
    }
    public IActionResult ChangePasswordPage()
    {
        return View();
    }
}
