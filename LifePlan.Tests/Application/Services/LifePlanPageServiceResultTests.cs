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

    [Fact]
    public void Submit_RejectsFractionalMoneyInput()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.Savings.CurrentFinancialAssetsManYen = 3.2m;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Key == "Savings.CurrentFinancialAssetsManYen");
        Assert.Null(result.Page.Result);
    }

    [Fact]
    public void Submit_RejectsUndefinedAnnualIncomeChangeRate()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.IncomeExpense.HusbandIncome.AnnualIncomeChangeRatePercent = 3m;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Key == "IncomeExpense.HusbandIncome.AnnualIncomeChangeRatePercent");
        Assert.Null(result.Page.Result);
    }

    [Fact]
    public void Submit_RejectsHousingInterestRateWithMoreThanOneDecimalPlace()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.LifeEvents.Housing.PurchaseHusbandAge = 35;
        input.LifeEvents.Housing.InterestRatePercent = 1.22m;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Key == "LifeEvents.Housing.InterestRatePercent");
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
