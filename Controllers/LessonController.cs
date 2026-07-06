using HonoursProject.Services;
using HonoursProject.Models;
using Microsoft.AspNetCore.Mvc;

public class LessonController : Controller
{
    private readonly LessonService _lessonService;

    public LessonController(LessonService lessonService)
    {
        _lessonService = lessonService;
    }

    public async Task<IActionResult> LessonSelectionPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage", "Account");

        // ✔ FIX: Load lessons dynamically
        var lessons = await _lessonService.GetAllLessonsAsync();
        return View(lessons);
    }

    public async Task<IActionResult> View(int id)
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage", "Account");

        // ✔ Load the lesson from the database
        var lesson = await _lessonService.GetLessonByIdAsync(id);

        if (lesson == null)
            return NotFound();

        // ✔ Pass the lesson model into LessonPage.cshtml
        return View("LessonPage", lesson);
    }
    public async Task<IActionResult> Start(int id)
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage", "Account");

        var questions = await _lessonService.GetQuestionsForLessonAsync(id);

        return View("LessonRunner", questions);
    }


    public async Task<IActionResult> Index()
    {
        var lessons = await _lessonService.GetAllLessonsAsync();
        return View(lessons);
    }
}
