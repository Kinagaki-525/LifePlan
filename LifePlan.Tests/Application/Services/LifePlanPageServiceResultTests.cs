using LifePlan.Application.Services;
using LifePlan.Domain.ReferenceData;
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

    [Fact]
    public void Submit_ShowsSpecVersionNoticeWithoutCalculatingWhenVersionIsMissing()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.CalculationSpecVersion = null;
        input.IncomeExpense.Expenses.MonthlyBasicLivingCostManYen = 20m;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.True(result.Page.SpecVersionNotice);
        Assert.Empty(result.Errors);
        Assert.Null(result.Page.Result);
        Assert.Null(result.CalculationResult);
        Assert.Equal(20m, result.Page.IncomeExpense.Expenses.MonthlyBasicLivingCostManYen);
        Assert.Equal("2", result.Page.CalculationSpecVersion);
    }

    [Fact]
    public void Submit_TreatsHousingWithoutPurchaseCostAsNoPurchase()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.IncomeExpense.Expenses.MonthlyRentManYen = 10m;
        input.LifeEvents.Housing.PurchaseHusbandAge = 30;
        input.LifeEvents.Housing.DownPaymentManYen = 0m;
        input.LifeEvents.Housing.BorrowingAmountManYen = 0m;
        input.LifeEvents.Housing.LoanYears = 35;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.True(result.IsValid);
        Assert.NotNull(result.CalculationResult);
        Assert.All(result.CalculationResult.AnnualRows, row =>
        {
            Assert.Equal(1_200_000, row.Expenses.RentYen);
            Assert.Equal(0, row.Expenses.HousingMaintenanceYen);
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Submit_TreatsBlankVersionAsMissing(string version)
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.CalculationSpecVersion = version;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.True(result.Page.SpecVersionNotice);
        Assert.Empty(result.Errors);
        Assert.Null(result.CalculationResult);
        Assert.Equal("2", result.Page.CalculationSpecVersion);
    }

    [Fact]
    public void Submit_ShowsSpecVersionNoticeWhenVersionIsMissingWithBindingErrors()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.CalculationSpecVersion = null;

        var result = service.Submit(input, hasBindingErrors: true);

        Assert.False(result.IsValid);
        Assert.True(result.Page.SpecVersionNotice);
        Assert.Null(result.CalculationResult);
        Assert.Equal("2", result.Page.CalculationSpecVersion);
    }

    [Fact]
    public void Submit_SetsChildMonthlyReferenceFromSubmittedAges()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.Family.Children = [new ChildInputViewModel { Age = 0 }];
        input.Savings.CurrentFinancialAssetsManYen = -1m;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.NotNull(result.Page.ExpenseGuidance.ChildMonthlyReference);
    }

    [Fact]
    public void CreateInitialPage_DoesNotSetChildMonthlyReference()
    {
        var page = new LifePlanPageService().CreateInitialPage();

        Assert.Null(page.ExpenseGuidance.ChildMonthlyReference);
        Assert.NotEmpty(page.ExpenseGuidance.ChildMonthlyReferenceByAgeBand);
    }

    [Fact]
    public void Submit_ReturnsValidationErrorsWithSpecVersionNoticeWhenVersionIsMissing()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.CalculationSpecVersion = null;
        input.Savings.CurrentFinancialAssetsManYen = -1m;

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.True(result.Page.SpecVersionNotice);
        Assert.Contains(result.Errors, error => error.Key == "Savings.CurrentFinancialAssetsManYen");
        Assert.Null(result.Page.Result);
    }

    [Fact]
    public void Submit_RejectsUndefinedCalculationSpecVersion()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();
        input.CalculationSpecVersion = "99";

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.False(result.IsValid);
        Assert.False(result.Page.SpecVersionNotice);
        Assert.Contains(result.Errors, error => error.Key == "CalculationSpecVersion");
        Assert.Null(result.Page.Result);
    }

    [Fact]
    public void Submit_CalculatesWithServerAssumptionsWhenVersionIsCurrent()
    {
        var service = new LifePlanPageService();
        var input = CreateValidInput();

        var result = service.Submit(input, hasBindingErrors: false);

        Assert.True(result.IsValid);
        Assert.False(result.Page.SpecVersionNotice);
        Assert.NotNull(result.CalculationResult);
        Assert.Equal(SimulationAssumptions.Current.Version, result.CalculationResult.AssumptionsVersion);
    }

    [Fact]
    public void CreateInitialPage_SetsCurrentCalculationSpecVersion()
    {
        var page = new LifePlanPageService().CreateInitialPage();

        Assert.Equal("2", page.CalculationSpecVersion);
        Assert.False(page.SpecVersionNotice);
    }

    private static LifePlanViewModel CreateValidInput()
    {
        return new LifePlanViewModel
        {
            CalculationSpecVersion = "2",
            Family = new FamilyInputViewModel
            {
                HusbandAge = 30,
                WifeAge = 30
            }
        };
    }
}
