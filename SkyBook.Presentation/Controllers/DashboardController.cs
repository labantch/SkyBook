using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;

namespace SkyBook.Presentation.Controllers
{
    
    
     
        [Authorize(Roles = "Admin")]
        public class DashboardController : Controller
        {
            private readonly IDashboardService _dashboardService;

            public DashboardController(
                IDashboardService dashboardService)
            {
                _dashboardService = dashboardService;
            }

            public async Task<IActionResult> Index()
            {
                var model =
                    await _dashboardService
                        .GetDashboardDataAsync();

                return View(model);
            }
        }
    }


