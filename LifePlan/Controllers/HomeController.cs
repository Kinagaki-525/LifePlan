using LifePlan.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using LifePlan.Models;
using LifePlan.ViewModels.Home;
using System.Diagnostics;

namespace LifePlan.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAffiliateLinkService affiliateLinkService;

        public HomeController(IAffiliateLinkService affiliateLinkService)
        {
            this.affiliateLinkService = affiliateLinkService;
        }

        public IActionResult Index()
        {
            return View(new HomeIndexViewModel
            {
                FpConsultationLink = affiliateLinkService.GetFpConsultationLink()
            });
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
}
