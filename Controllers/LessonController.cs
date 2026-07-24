using HonoursProject.Services;
using HonoursProject.Models;
using Microsoft.AspNetCore.Mvc;

public class LessonController : Controller
{
    private readonly LessonService _lessonService;
    private readonly AchievementService _achievementService;

    public LessonController(LessonService lessonService, AchievementService achievementService)
    {
        _lessonService = lessonService;
        _achievementService = achievementService;
    }

    public async Task<IActionResult> LessonSelectionPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage", "Account");

        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null)
            return RedirectToAction("LoginPage", "Account");

        // 1. Get all lessons
        var lessons = await _lessonService.GetAllLessonsAsync();

        // 2. Get completed lesson IDs for this user
        var completedLessonIds = await _lessonService.GetCompletedLessonIdsAsync(userId.Value);

        // 3. Sort lessons by order_index
        lessons = lessons.OrderBy(l => l.order_index).ToList();

        // 4. Build the view model list
        var model = new List<LessonProgressViewModel>();

        for (int i = 0; i < lessons.Count; i++)
        {
            var lesson = lessons[i];

            bool isCompleted = completedLessonIds.Contains(lesson.id);

            bool isUnlocked = i == 0 || completedLessonIds.Contains(lessons[i - 1].id);

            model.Add(new LessonProgressViewModel
            {
                Lesson = lesson,
                IsCompleted = isCompleted,
                IsUnlocked = isUnlocked
            });
        }

        var exam = model.FirstOrDefault(m => m.Lesson.is_exam);
        var normalLessons = model.Where(m => !m.Lesson.is_exam).ToList();

        var pageModel = new LessonSelectionPageViewModel
        {
            Lessons = normalLessons,
            Exam = exam
        };

        return View(pageModel);
    }


    public async Task<IActionResult> View(int id)
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
            return RedirectToAction("LoginPage", "Account");

        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null)
            return RedirectToAction("LoginPage", "Account");

        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null)
            return NotFound();

        // Get all lessons
        var lessons = await _lessonService.GetAllLessonsAsync();
        lessons = lessons.OrderBy(l => l.order_index).ToList();

        // Get completed lessons
        var completedLessonIds = await _lessonService.GetCompletedLessonIdsAsync(userId.Value);

        // Build unlocked lesson list
        var unlockedLessons = new List<Lesson>();
        for (int i = 0; i < lessons.Count; i++)
        {
            bool isUnlocked = i == 0 || completedLessonIds.Contains(lessons[i - 1].id);
            if (isUnlocked)
                unlockedLessons.Add(lessons[i]);
        }

        // Find current index within unlocked lessons
        int currentIndex = unlockedLessons.FindIndex(l => l.id == id);

        int? previousId = currentIndex > 0 ? unlockedLessons[currentIndex - 1].id : null;
        int? nextId = currentIndex < unlockedLessons.Count - 1 ? unlockedLessons[currentIndex + 1].id : null;

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
        ViewBag.LessonId = id;
        return View("LessonRunner", questions);
    }

    public async Task<IActionResult> Index()
    {
        var lessons = await _lessonService.GetAllLessonsAsync();
        return View(lessons);
    }

    public async Task<IActionResult> Complete()
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
        await _lessonService.SaveLessonHistory(userId.Value, lessonId, score, durationSeconds);

        // -----------------------------
        // 4. Award XP
        // -----------------------------
        int xp = score;

        if (durationSeconds < 60) xp += 25;   // speed bonus
        if (durationSeconds < 30) xp += 50;  // lightning bonus

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
        // 7. Unlock achievements
        // -----------------------------
        var unlocked = new List<AchievementViewModel>();

        unlocked.AddRange(await _achievementService.UnlockAchievementsAsync(userId.Value, "progression"));
        unlocked.AddRange(await _achievementService.UnlockAchievementsAsync(userId.Value, "lesson"));
        unlocked.AddRange(await _achievementService.UnlockAchievementsAsync(userId.Value, "engagement"));
        unlocked.AddRange(await _achievementService.UnlockAchievementsAsync(userId.Value, "performance"));
        unlocked.AddRange(await _achievementService.UnlockAchievementsAsync(userId.Value, "special"));
        unlocked.AddRange(await _achievementService.UnlockAchievementsAsync(userId.Value, "level"));
        unlocked.AddRange(await _achievementService.UnlockAchievementsAsync(userId.Value, "streak"));

        ViewBag.UnlockedAchievements = unlocked;

        // -----------------------------
        // 8. Pass data to the view
        // -----------------------------
        ViewBag.Score = score;
        ViewBag.Xp = xp;
        ViewBag.Duration = durationSeconds;

        return View();
    }

    [HttpPost]
    public IActionResult RecordAttempt([FromBody] QuestionAttemptDto attempt)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (userId == null)
                return Unauthorized();

            _lessonService.RecordQuestionAttempt(
                userId.Value,
                attempt.LessonId,
                attempt.QuestionId,
                attempt.OptionId ?? 0,
                attempt.IsCorrect,
                attempt.TimeTakenSeconds,
                attempt.Attempts
            );

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("RecordAttempt ERROR: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, ex.Message);
        }
    }

    public class QuestionAttemptDto
    {
        public int LessonId { get; set; }
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public bool IsCorrect { get; set; }
        public int TimeTakenSeconds { get; set; }
        public int Attempts { get; set; }
    }
}
