using LifePlan.Application.Interfaces;
using LifePlan.ViewModels.Contact;
using Microsoft.AspNetCore.Mvc;

namespace LifePlan.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactPageService contactPageService;

        public ContactController(IContactPageService contactPageService)
        {
            this.contactPageService = contactPageService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(contactPageService.CreateInitialPage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel model)
        {
            var result = await contactPageService.Submit(model, !ModelState.IsValid);

            if (result.IsSent)
            {
                return RedirectToAction(nameof(Thanks));
            }

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
            }

            return View(result.Page);
        }

        [HttpGet]
        public IActionResult Thanks() => View();
    }
}
