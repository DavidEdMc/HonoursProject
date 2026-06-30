using Microsoft.AspNetCore.Mvc;
using HonoursProject.Services;

public class DashboardController : Controller
{
    private readonly UserService _userService;

    public DashboardController(UserService userService)
    {
        _userService = userService;
    }

    public IActionResult DashboardPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        ViewBag.Username = username;

        var leaderboard = _userService.GetLeaderboard();
        return View(leaderboard);
    }

}
