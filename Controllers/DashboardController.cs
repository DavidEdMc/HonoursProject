using Microsoft.AspNetCore.Mvc;
using HonoursProject.Services;
using HonoursProject.Models;

public class DashboardController : Controller
{
    private readonly UserService _userService;
    private readonly LessonService _lessonService;
    private readonly AchievementService _achievementService;

    public DashboardController(
        UserService userService,
        LessonService lessonService,
        AchievementService achievementService)
    {
        _userService = userService;
        _lessonService = lessonService;
        _achievementService = achievementService;
    }

    public async Task<IActionResult> DashboardPage()
    {
        var username = HttpContext.Session.GetString("username");
        var userId = HttpContext.Session.GetInt32("user_id");

        if (username == null || userId == null)
            return RedirectToAction("LoginPage", "Account");

        var model = new DashboardViewModel
        {
            Username = username,
            LessonsCompleted = await _lessonService.GetTotalLessonsCompletedAsync(userId.Value),
            AchievementsEarned = await _achievementService.GetUserAchievementCount(userId.Value),
            DayStreak = await _lessonService.GetUserStreakAsync(userId.Value),
            Leaderboard = _userService.GetLeaderboard()
        };

        return View(model);
    }


}
