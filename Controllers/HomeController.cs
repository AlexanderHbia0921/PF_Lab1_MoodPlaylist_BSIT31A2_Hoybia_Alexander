using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MoodPlaylistGenerator.Data.Entities;
using MoodPlaylistGenerator.Services.Implementations;
using MoodPlaylistGenerator.Services.Models;
using System.Security.Claims;

namespace MoodPlaylistGenerator.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AnalyticsService _analyticsService;

    public HomeController(ILogger<HomeController> logger, AnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId == 0)
        {
            throw new UnauthorizedAccessException("User is not properly authenticated or user ID is invalid.");
        }
        return userId;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var userId = GetCurrentUserId();
            var dashboardData = await _analyticsService.GetDashboardAnalyticsAsync(userId);
            ViewData["Title"] = "Dashboard";
            return View(dashboardData);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Account");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
