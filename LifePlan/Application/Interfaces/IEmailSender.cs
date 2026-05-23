using LifePlan.Application.Dto;

namespace LifePlan.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendContactAsync(ContactEmailMessage message);
    }
}
