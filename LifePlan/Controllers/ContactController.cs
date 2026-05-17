using LifePlan.Application.Interfaces;
using LifePlan.Models;
using Microsoft.AspNetCore.Mvc;

namespace LifePlan.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailSender emailSender, ILogger<ContactController> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() => View(new ContactViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _emailSender.SendContactAsync(model);
                return RedirectToAction(nameof(Thanks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "メール送信失敗");
                ModelState.AddModelError(string.Empty, "送信に失敗しました。しばらくしてから再度お試しください。"
                );
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Thanks() => View();
    }
}