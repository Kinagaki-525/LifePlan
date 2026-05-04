using LifePlan.Application.Services;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Tests.Application.Services;

public class LifePlanPageServiceResultTests
{
    [Fact]
    public void Submit_SetsResultWhenInputIsValid()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Page.Result);
        Assert.NotEmpty(result.Page.Result.CashFlowRows);
    }

    [Fact]
    public void Submit_DoesNotSetResultWhenBindingErrorsExist()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();

        var result = service.Submit(input, hasBindingErrors: true);

        Assert.False(result.IsValid);
        Assert.Null(result.Page.Result);
    }

    [Fact]
    public void Submit_DoesNotSetResultWhenValidationErrorsExist()
    {
        var service = new LifePlanPageService();
        var input = new LifePlanViewModel();

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.Null(result.Page.Result);
    }

    private static LifePlanViewModel CreateValidInput()
    {
        return new LifePlanViewModel
        {
            Family = new FamilyInputViewModel
            {
                HusbandAge = 30,
                WifeAge = 30
            }
        };
    }
}
