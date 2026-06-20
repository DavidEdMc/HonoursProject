using Microsoft.AspNetCore.Mvc;
public class LessonController : Controller
{
    public IActionResult LessonSelectionPage()
    {
        return View();
    }

    public IActionResult LessonPage()
    {
        return View();
    }

}
