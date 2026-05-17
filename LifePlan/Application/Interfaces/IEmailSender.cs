using System.Threading.Tasks;
using LifePlan.Models;

namespace LifePlan.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendContactAsync(ContactViewModel model);
    }
}
