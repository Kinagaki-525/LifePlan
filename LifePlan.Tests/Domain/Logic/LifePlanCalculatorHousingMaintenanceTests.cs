using LifePlan.Domain.Entities;
using LifePlan.Domain.Logic;

namespace LifePlan.Tests.Domain.Logic;

public class LifePlanCalculatorHousingMaintenanceTests
{
    private const int CurrentYear = 2026;

    [Fact]
    public void Calculate_AddsHousingMaintenanceFromFirstYearPurchase()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = CreateHousing(purchaseHusbandAge: 30, downPaymentYen: 10_000_000, borrowingAmountYen: 40_000_000);

        var result = Calculate(input);

        Assert.Equal(500_000, result.AnnualRows[0].Expenses.HousingMaintenanceYen);
        Assert.Equal(510_000, result.AnnualRows[1].Expenses.HousingMaintenanceYen);
    }

    [Fact]
    public void Calculate_AddsHousingMaintenanceFromFuturePurchaseYearWithInflation()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = CreateHousing(purchaseHusbandAge: 35, downPaymentYen: 10_000_000, borrowingAmountYen: 40_000_000);

        var result = Calculate(input);

        Assert.All(result.AnnualRows.Take(5), row => Assert.Equal(0, row.Expenses.HousingMaintenanceYen));
        Assert.Equal(552_040, result.AnnualRows[5].Expenses.HousingMaintenanceYen);
    }

    [Fact]
    public void Calculate_ContinuesHousingMaintenanceAfterLoanIsRepaid()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = CreateHousing(purchaseHusbandAge: 30, downPaymentYen: 10_000_000, borrowingAmountYen: 40_000_000);
        input.LifeEvents.Housing.LoanYears = 2;

        var result = Calculate(input);

        Assert.Equal(0, result.AnnualRows[2].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(520_200, result.AnnualRows[2].Expenses.HousingMaintenanceYen);
    }

    [Fact]
    public void Calculate_DoesNotAddHousingMaintenanceWhenPurchaseCostIsZero()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = CreateHousing(purchaseHusbandAge: 30, downPaymentYen: 0, borrowingAmountYen: 0);

        var result = Calculate(input);

        Assert.All(result.AnnualRows, row => Assert.Equal(0, row.Expenses.HousingMaintenanceYen));
    }

    [Fact]
    public void Calculate_DoesNotAddHousingMaintenanceWithoutPurchase()
    {
        var result = Calculate(CreateInput());

        Assert.All(result.AnnualRows, row => Assert.Equal(0, row.Expenses.HousingMaintenanceYen));
    }

    [Fact]
    public void Calculate_AddsHousingMaintenanceFromCurrentYearForPastPurchase()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = CreateHousing(purchaseHusbandAge: 25, downPaymentYen: 30_000_000, borrowingAmountYen: 0);

        var result = Calculate(input);

        Assert.Equal(300_000, result.AnnualRows[0].Expenses.HousingMaintenanceYen);
        Assert.Equal(0, result.AnnualRows[0].Expenses.HousingDownPaymentYen);
    }

    [Fact]
    public void Calculate_AddsHousingMaintenanceToExpenseTotal()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = CreateHousing(purchaseHusbandAge: 30, downPaymentYen: 10_000_000, borrowingAmountYen: 0);

        var result = Calculate(input);

        Assert.Equal(10_100_000, result.AnnualRows[0].TotalExpenseYen);
    }

    private static HousingEventData CreateHousing(int purchaseHusbandAge, long downPaymentYen, long borrowingAmountYen)
    {
        return new HousingEventData
        {
            PurchaseHusbandAge = purchaseHusbandAge,
            DownPaymentYen = downPaymentYen,
            BorrowingAmountYen = borrowingAmountYen,
            LoanYears = 35,
            InterestRatePercent = 0m
        };
    }

    private static LifePlanCalculationResult Calculate(LifePlanData input)
    {
        return new LifePlanCalculator().Calculate(input, CurrentYear);
    }

    private static LifePlanData CreateInput()
    {
        return new LifePlanData
        {
            Family = new FamilyData
            {
                HusbandAge = 30,
                WifeAge = 30
            },
            LifeEvents = new LifeEventData(),
            IncomeExpense = new IncomeExpenseData
            {
                Expenses = new ExpenseData()
            }
        };
    }
}
