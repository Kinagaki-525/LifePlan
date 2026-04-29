using Microsoft.AspNetCore.Mvc;
using rennsyu.Application.Interfaces;
using rennsyu.ViewModels.LifePlan;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LifePlanViewModel model)
        {
            var result = lifePlanPageService.Submit(model, !ModelState.IsValid);

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Key, error.Message);
            }

            return View(result.Page);
        }
    }
}
