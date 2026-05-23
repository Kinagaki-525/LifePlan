using LifePlan.Application.Interfaces;
using LifePlan.Application.Mappers;
using LifePlan.Application.Results;
using LifePlan.ViewModels.Contact;

namespace LifePlan.Application.Services
{
    public sealed class ContactPageService : IContactPageService
    {
        private readonly IEmailSender emailSender;
        private readonly ILogger<ContactPageService> logger;

        public ContactPageService(IEmailSender emailSender, ILogger<ContactPageService> logger)
        {
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public ContactViewModel CreateInitialPage()
        {
            return new ContactViewModel();
        }

        public async Task<ContactSubmitResult> Submit(ContactViewModel input, bool hasBindingErrors)
        {
            if (hasBindingErrors)
            {
                return new ContactSubmitResult
                {
                    Page = input,
                    IsSent = false
                };
            }

            try
            {
                await emailSender.SendContactAsync(ContactEmailMessageMapper.ToEmailMessage(input));

                return new ContactSubmitResult
                {
                    Page = input,
                    IsSent = true
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "お問い合わせメールの送信に失敗しました。");

                return new ContactSubmitResult
                {
                    Page = input,
                    IsSent = false,
                    ErrorMessage = "送信に失敗しました。しばらくしてから再度お試しください。"
                };
            }
        }
    }
}
