using LifePlan.Application.Results;
using LifePlan.ViewModels.Contact;

namespace LifePlan.Application.Interfaces
{
    public interface IContactPageService
    {
        ContactViewModel CreateInitialPage();

        Task<ContactSubmitResult> Submit(ContactViewModel input, bool hasBindingErrors);
    }
}
