using LifePlan.Domain.Entities;
using LifePlan.Domain.Logic;
using LifePlan.Domain.ReferenceData;

namespace LifePlan.Tests.Domain.Logic;

public class LifePlanCalculatorSavingsTests
{
    private const int CurrentYear = 2026;

    [Fact]
    public void Calculate_UsesCurrentFinancialAssetsAsInitialStartingAssets()
    {
        var input = CreateInput();
        input.Assets.CurrentFinancialAssetsYen = 3_000_000;

        var result = Calculate(input);

        Assert.Equal(3_000_000, result.AnnualRows[0].StartingAssetsYen);
    }

    [Fact]
    public void Calculate_CalculatesAnnualBalanceFromIncomeAndExpenseTotals()
    {
        var input = CreateInput();
        input.IncomeExpense.HusbandIncome = new PersonIncomeData
        {
            AnnualIncomeYen = 5_000_000,
            WorkStartAge = 30,
            WorkEndAge = 30
        };
        input.IncomeExpense.Expenses.MonthlyBasicLivingCostYen = 100_000;

        var result = Calculate(input);

        Assert.Equal(5_000_000, result.AnnualRows[0].TotalIncomeYen);
        Assert.Equal(1_200_000, result.AnnualRows[0].TotalExpenseYen);
        Assert.Equal(3_800_000, result.AnnualRows[0].AnnualBalanceYen);
    }

    [Fact]
    public void Calculate_CumulatesSavingsBalanceWithoutReturn()
    {
        var input = CreateInput();
        input.Assets.CurrentFinancialAssetsYen = 1_000_000;
        input.IncomeExpense.HusbandIncome = new PersonIncomeData
        {
            AnnualIncomeYen = 1_000_000,
            WorkStartAge = 30,
            WorkEndAge = 31
        };
        input.IncomeExpense.Expenses.MonthlyBasicLivingCostYen = 50_000;

        var result = Calculate(input);

        Assert.Equal(400_000, result.AnnualRows[0].AnnualBalanceYen);
        Assert.Equal(1_400_000, result.AnnualRows[0].SavingsBalanceWithoutReturnYen);
        Assert.Equal(1_400_000, result.AnnualRows[1].StartingAssetsYen);
        Assert.Equal(1_788_000, result.AnnualRows[1].SavingsBalanceWithoutReturnYen);
    }

    [Fact]
    public void Calculate_AppliesExpectedAnnualReturnToPreviousBalanceBeforeAnnualBalance()
    {
        var input = CreateInput();
        input.Assets.CurrentFinancialAssetsYen = 1_000_000;
        input.Assets.ExpectedAnnualReturnRatePercent = 10m;
        input.IncomeExpense.HusbandIncome = new PersonIncomeData
        {
            AnnualIncomeYen = 200_000,
            WorkStartAge = 30,
            WorkEndAge = 31
        };

        var result = Calculate(input);

        Assert.Equal(1_300_000, result.AnnualRows[0].SavingsBalanceWithReturnYen);
        Assert.Equal(1_630_000, result.AnnualRows[1].SavingsBalanceWithReturnYen);
    }

    [Fact]
    public void Calculate_RoundsExpectedAnnualReturnToYen()
    {
        var input = CreateInput();
        input.Assets.CurrentFinancialAssetsYen = 1;
        input.Assets.ExpectedAnnualReturnRatePercent = 50m;

        var result = Calculate(input);

        Assert.Equal(2, result.AnnualRows[0].SavingsBalanceWithReturnYen);
    }

    [Fact]
    public void Calculate_BalanceDifferenceEqualsCumulativeAutomaticExpenses()
    {
        var input = CreateInput();
        input.Assets.CurrentFinancialAssetsYen = 10_000_000;
        input.Assets.ExpectedAnnualReturnRatePercent = 0m;
        input.IncomeExpense.Expenses.MonthlyBasicLivingCostYen = 200_000;
        input.Family.Children = [new ChildData { Age = -1 }, new ChildData { Age = 10 }];
        input.LifeEvents.Housing = new HousingEventData
        {
            PurchaseHusbandAge = 33,
            DownPaymentYen = 5_000_000,
            BorrowingAmountYen = 20_000_000,
            LoanYears = 20,
            InterestRatePercent = 1m
        };
        var withoutAutomaticExpenses = SimulationAssumptions.Current with
        {
            HousingMaintenanceRatePercent = 0m,
            ChildLivingCosts = []
        };

        var baseline = new LifePlanCalculator(withoutAutomaticExpenses).Calculate(input, CurrentYear);
        var result = Calculate(input);
        var automaticExpensesYen = result.AnnualRows.Sum(row =>
            row.Expenses.ChildLivingCostYen + row.Expenses.HousingMaintenanceYen);

        Assert.True(automaticExpensesYen > 0);
        Assert.Equal(
            automaticExpensesYen,
            baseline.AnnualRows[^1].SavingsBalanceWithoutReturnYen - result.AnnualRows[^1].SavingsBalanceWithoutReturnYen);
        Assert.Equal(
            automaticExpensesYen,
            baseline.AnnualRows[^1].SavingsBalanceWithReturnYen - result.AnnualRows[^1].SavingsBalanceWithReturnYen);
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
