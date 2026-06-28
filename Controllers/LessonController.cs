using Microsoft.AspNetCore.Mvc;
public class LessonController : Controller
{
    public IActionResult LessonSelectionPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        return View();
    }


    public IActionResult LessonPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }
        
        return View();
    }

}
