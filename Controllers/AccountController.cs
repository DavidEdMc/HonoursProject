using Microsoft.AspNetCore.Mvc;
using HonoursProject.Services;
using HonoursProject.Models;
using System.Security.Cryptography;

public class AccountController : Controller
{
    private readonly UserService _userService;
    private readonly AchievementService _achievementService;
    private readonly LessonService _lessonService;

    public AccountController(UserService userService, AchievementService achievementService, LessonService lessonService)
    {
        _userService = userService;
        _achievementService = achievementService;
        _lessonService = lessonService;
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

        if (!_userService.VerifyPassword(password, user.Password))
        {
            ViewBag.Error = "Invalid username or password";
            return View("LoginPage");
        }

        // Login success
        HttpContext.Session.SetString("username", user.Username);
        HttpContext.Session.SetString("profile_picture", user.ProfilePicture);
        HttpContext.Session.SetInt32("user_id", user.Id);

        return RedirectToAction("Index", "Home");
    }


    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(string EmailAddress)
    {
        var user = _userService.GetUserByEmail(EmailAddress);

        if (user == null)
        {
            TempData["ResetMessage"] = "No account found with that email.";
            return RedirectToAction("ForgotPassword");
        }

        // Store email temporarily
        TempData["ResetEmail"] = EmailAddress;

        return RedirectToAction("ResetPasswordPage");
    }

    public IActionResult ResetPasswordPage()
    {
        // If TempData was lost (e.g., direct navigation), redirect back
        if (TempData["ResetEmail"] == null)
            return RedirectToAction("ForgotPassword");

        return View();
    }


    [HttpPost]
    public IActionResult ResetPassword(string EmailAddress, string NewPassword, string ConfirmPassword)
    {
        if (NewPassword != ConfirmPassword)
        {
            TempData["ResetError"] = "Passwords do not match.";
            TempData["ResetEmail"] = EmailAddress;
            return RedirectToAction("ResetPasswordPage");
        }

        var user = _userService.GetUserByEmail(EmailAddress);

        if (user == null)
        {
            TempData["ResetError"] = "Account not found.";
            return RedirectToAction("ForgotPassword");
        }

        var newHash = _userService.HashPassword(NewPassword);
        _userService.UpdatePassword(user.Username, newHash);

        return RedirectToAction("ResetConfirmation");
    }

    public IActionResult ResetConfirmation()
    {
        return View();
    }

    public async Task<IActionResult> ProfilePage()
    {
        var username = HttpContext.Session.GetString("username");
        var userId = HttpContext.Session.GetInt32("user_id");

        if (username == null || userId == null)
            return RedirectToAction("LoginPage", "Account");

        var user = _userService.GetUserByUsername(username);

        int xp = await _lessonService.GetUserXpAsync(userId.Value);
        int level = await _lessonService.GetUserLevelAsync(userId.Value);

        int xpToNextLevel = (level * 100) - xp; // matches your existing formula

        var model = new ProfileViewModel
        {
            Username = user.Username,
            EmailAddress = user.EmailAddress,
            ProfilePicture = user.ProfilePicture,

            LessonsCompleted = await _lessonService.GetTotalLessonsCompletedAsync(userId.Value),
            AchievementsEarned = await _achievementService.GetUserAchievementCount(userId.Value),
            DayStreak = await _lessonService.GetUserStreakAsync(userId.Value),

            CurrentXp = xp,
            Level = level,
            XpToNextLevel = xpToNextLevel
        };

        return View(model);
    }

    public IActionResult AchievementsPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        var user = _userService.GetUserByUsername(username);

        var achievementsList = _achievementService.GetAllAchievementsForUser(user.Id);

        return View(achievementsList);
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
            return RedirectToAction("LoginPage");

        var user = _userService.GetUserByUsername(username);
        return View(user);
    }

    [HttpPost]
    public IActionResult EditProfile(string Username, string EmailAddress, IFormFile AvatarFile)
    {
        var currentUsername = HttpContext.Session.GetString("username");
        if (currentUsername == null)
            return RedirectToAction("LoginPage");

        // Get current user
        var user = _userService.GetUserByUsername(currentUsername);

        // Update username + email
        user.Username = Username;
        user.EmailAddress = EmailAddress;

        // Handle profile picture upload
        if (AvatarFile != null && AvatarFile.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
            var filePath = Path.Combine("wwwroot/assets/profile_avatar", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                AvatarFile.CopyTo(stream);
            }

            user.ProfilePicture = "/assets/profile_avatar/" + fileName;
        }

        // Save changes
        _userService.UpdateProfile(user);

        // Update session if username or picture changed
        HttpContext.Session.SetString("username", user.Username);
        HttpContext.Session.SetString("profile_picture", user.ProfilePicture);

        TempData["ProfileMessage"] = "Profile updated successfully!";
        return RedirectToAction("EditProfilePage");
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

    public IActionResult RegisterPage()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string Username, string EmailAddress, string Password)
    {
        // Check if username already exists
        var existingUser = _userService.GetUserByUsername(Username);
        if (existingUser != null)
        {
            TempData["RegisterMessage"] = "Username already taken.";
            return RedirectToAction("RegisterPage");
        }

        // Check if email already exists
        var existingEmail = _userService.GetUserByEmail(EmailAddress);
        if (existingEmail != null)
        {
            TempData["RegisterMessage"] = "Email address is already registered.";
            return RedirectToAction("RegisterPage");
        }

        // Hash password
        var hash = _userService.HashPassword(Password);

        // Create user object
        var newUser = new User
        {
            Username = Username,
            EmailAddress = EmailAddress,
            Password = hash,
            ProfilePicture = "/assets/images/default-avatar.png" // default avatar
        };

        // Save to DB
        _userService.CreateUser(newUser);

        // Auto-login
        HttpContext.Session.SetString("username", newUser.Username);
        HttpContext.Session.SetString("profile_picture", newUser.ProfilePicture);

        return RedirectToAction("ProfilePage");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("LoginPage");
    }
}
