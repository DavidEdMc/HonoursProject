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

        var lessons = await _lessonService.GetAllLessonsAsync();
        return View(lessons);
    }

    public async Task<IActionResult> View(int id)
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage", "Account");

        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null)
            return NotFound();

        // Get all lessons so we can find previous/next
        var lessons = await _lessonService.GetAllLessonsAsync();

        // Sort by order_index
        lessons = lessons.OrderBy(l => l.order_index).ToList();

        // Find current index
        int currentIndex = lessons.FindIndex(l => l.id == id);

        // Determine previous and next
        int? previousId = currentIndex > 0 ? lessons[currentIndex - 1].id : null;
        int? nextId = currentIndex < lessons.Count - 1 ? lessons[currentIndex + 1].id : null;

        ViewBag.PreviousId = previousId;
        ViewBag.NextId = nextId;

        return View("LessonPage", lesson);
    }

    public async Task<IActionResult> Start(int id, int index = 0)
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage", "Account");
        
        HttpContext.Session.SetString("LessonStartTime", DateTime.UtcNow.ToString());
        TempData["LessonId"] = id;

        var questions = await _lessonService.GetQuestionsForLessonAsync(id);

        if (index < 0 || index >= questions.Count)
            index = 0;

        ViewBag.Index = index;
        return View("LessonRunner", questions);
    }

    public async Task<IActionResult> Index()
    {
        var lessons = await _lessonService.GetAllLessonsAsync();
        return View(lessons);
    }

    public IActionResult Complete()
    {
        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null)
            return RedirectToAction("LoginPage", "Account");

        // Retrieve start time
        var startString = HttpContext.Session.GetString("LessonStartTime");

        // Retrieve lessonId from TempData
        var lessonIdString = TempData["LessonId"]?.ToString();
        if (!int.TryParse(lessonIdString, out int lessonId))
        {
            HttpContext.Session.Remove("LessonStartTime");
            return View();
        }

        // -----------------------------
        // 1. Calculate duration
        // -----------------------------
        int durationSeconds = 0;
        if (DateTime.TryParse(startString, out var startTime))
        {
            var endTime = DateTime.UtcNow;
            durationSeconds = (int)(endTime - startTime).TotalSeconds;
        }

        // -----------------------------
        // 2. Calculate score
        // -----------------------------
        int score = _lessonService.CalculateLessonScore(userId.Value, lessonId);

        // -----------------------------
        // 3. Save lesson history
        // -----------------------------
        _lessonService.SaveLessonHistory(userId.Value, lessonId, score, durationSeconds);

        // -----------------------------
        // 4. Award XP
        // -----------------------------
        int xp = score;

        if (durationSeconds < 60) xp += 50;   // speed bonus
        if (durationSeconds < 30) xp += 100;  // lightning bonus

        _lessonService.AddXp(userId.Value, xp);

        // -----------------------------
        // 5. Update leaderboard stats
        // -----------------------------
        _lessonService.UpdateUserStats(userId.Value, lessonId, score, durationSeconds);

        // -----------------------------
        // 6. Cleanup
        // -----------------------------
        HttpContext.Session.Remove("LessonStartTime");

        // -----------------------------
        // 7. Pass data to the view
        // -----------------------------
        ViewBag.Score = score;
        ViewBag.Xp = xp;
        ViewBag.Duration = durationSeconds;

        return View();
    }

    [HttpPost]
    public IActionResult RecordAttempt([FromBody] QuestionAttemptDto attempt)
    {
        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null)
            return Unauthorized();

        _lessonService.RecordQuestionAttempt(
            userId.Value,
            attempt.QuestionId,
            attempt.OptionId,
            attempt.IsCorrect,
            attempt.TimeTakenSeconds,
            attempt.Attempts
        );

        return Ok();
    }

    public class QuestionAttemptDto
    {
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
        public bool IsCorrect { get; set; }
        public int TimeTakenSeconds { get; set; }
        public int Attempts { get; set; }
    }
}
