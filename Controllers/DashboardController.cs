using Microsoft.AspNetCore.Mvc;
public class DashboardController : Controller
{
    public IActionResult DashboardPage()
    {
        var username = HttpContext.Session.GetString("username");
        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        return View();
    }
}
