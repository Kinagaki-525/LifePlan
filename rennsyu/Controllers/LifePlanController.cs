using Microsoft.AspNetCore.Mvc;
using LifePlan.Application.Interfaces;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Controllers
{
    public class LifePlanController : Controller
    {
        private readonly ILifePlanPageService lifePlanPageService;

        public LifePlanController(ILifePlanPageService lifePlanPageService)
        {
            this.lifePlanPageService = lifePlanPageService;
        }

        // TODO: 機能実装完了後に削除 - 公開時はTOPページにリダイレクト
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        /*
        // TODO: 機能実装完了後に有効化
        public IActionResult Index()
        {
            return View(lifePlanPageService.CreateInitialPage());
        }
        */

        [HttpPost]
        [ValidateAntiForgeryToken]
        // TODO: 機能実装完了後に削除 - 公開時はTOPページにリダイレクト
        public IActionResult Index(LifePlanViewModel model)
        {
            return RedirectToAction("Index", "Home");
        }

        /*
        // TODO: 機能実装完了後に有効化
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
        */
    }
}
