using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public AdminDashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dashboardService.GetDashboardDataAsync();
            return View(model);
        }
    }
}
