using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HonoursProject.Models;
using HonoursProject.Services;

namespace HonoursProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserService _userService;

    public HomeController(ILogger<HomeController> logger, UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public IActionResult Index()
    {
        var username = HttpContext.Session.GetString("username");

        if (username == null)
        {
            return RedirectToAction("LoginPage", "Account");
        }

        var user = _userService.GetUserByUsername(username);

        return View(user);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }
}
