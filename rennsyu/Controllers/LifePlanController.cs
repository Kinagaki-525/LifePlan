using Microsoft.AspNetCore.Mvc;
using rennsyu.Application.Interfaces;

namespace rennsyu.Controllers
{
    public class LifePlanController : Controller
    {
        private readonly ILifePlanPageService lifePlanPageService;

        public LifePlanController(ILifePlanPageService lifePlanPageService)
        {
            this.lifePlanPageService = lifePlanPageService;
        }

        public IActionResult Index()
        {
            return View(lifePlanPageService.CreateInitialPage());
        }
    }
}
