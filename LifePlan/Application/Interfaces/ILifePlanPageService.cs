using LifePlan.Application.Results;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Interfaces
{
    public interface ILifePlanPageService
    {
        LifePlanViewModel CreateInitialPage();

        LifePlanSubmitResult Submit(LifePlanViewModel input, bool hasBindingErrors);
    }
}
