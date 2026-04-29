using rennsyu.Application.Results;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Interfaces
{
    public interface ILifePlanPageService
    {
        LifePlanViewModel CreateInitialPage();

        LifePlanSubmitResult Submit(LifePlanViewModel input, bool hasBindingErrors);
    }
}
