using LifePlan.ViewModels.Contact;

namespace LifePlan.Application.Results
{
    public sealed class ContactSubmitResult
    {
        public ContactViewModel Page { get; init; } = new();

        public bool IsSent { get; init; }

        public string? ErrorMessage { get; init; }
    }
}
