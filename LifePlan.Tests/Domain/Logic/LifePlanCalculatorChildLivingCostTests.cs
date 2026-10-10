using LifePlan.Domain.Entities;
using LifePlan.Domain.Logic;
using LifePlan.Domain.ReferenceData;

namespace LifePlan.Tests.Domain.Logic;

public class LifePlanCalculatorChildLivingCostTests
{
    private const int CurrentYear = 2026;

    [Fact]
    public void Calculate_DoesNotAddChildLivingCostWithoutChildren()
    {
        var result = new LifePlanCalculator().Calculate(CreateInput(), CurrentYear);

        Assert.All(result.AnnualRows, row => Assert.Equal(0, row.Expenses.ChildLivingCostYen));
    }

    [Fact]
    public void Calculate_AddsChildLivingCostFromBirthYear()
    {
        var input = CreateInput();
        input.Family.Children = [new ChildData { Age = -2 }];

        var result = CalculateWithTestMaster(input);

        Assert.Equal(0, result.AnnualRows[0].Expenses.ChildLivingCostYen);
        Assert.Equal(0, result.AnnualRows[1].Expenses.ChildLivingCostYen);
        Assert.Equal(374_544, result.AnnualRows[2].Expenses.ChildLivingCostYen);
    }

    [Fact]
    public void Calculate_AddsChildLivingCostWithoutDeductingBasicLivingCost()
    {
        var input = CreateInput();
        input.Family.Children = [new ChildData { Age = 0 }];
        input.IncomeExpense.Expenses.MonthlyBasicLivingCostYen = 200_000;

        var result = CalculateWithTestMaster(input);

        Assert.Equal(2_400_000, result.AnnualRows[0].Expenses.BasicLivingCostYen);
        Assert.Equal(360_000, result.AnnualRows[0].Expenses.ChildLivingCostYen);
        Assert.Equal(2_760_000, result.AnnualRows[0].TotalExpenseYen);
    }

    [Fact]
    public void Calculate_RoundsChildLivingCostPerChildBeforeSumming()
    {
        var input = CreateInput();
        input.Family.Children =
        [
            new ChildData { Age = 0 },
            new ChildData { Age = 0 },
            new ChildData { Age = 0 },
            new ChildData { Age = 0 }
        ];
        var assumptions = CreateTestAssumptions(annualCostYen: 333_333, priceBaseYear: CurrentYear - 1);

        var result = new LifePlanCalculator(assumptions).Calculate(input, CurrentYear);

        Assert.Equal(1_360_000, result.AnnualRows[0].Expenses.ChildLivingCostYen);
    }

    [Fact]
    public void Calculate_StopsChildLivingCostAtSupportEndAge()
    {
        var input = CreateInput();
        input.Family.Children = [new ChildData { Age = 21 }];

        var result = CalculateWithTestMaster(input);

        Assert.Equal(360_000, result.AnnualRows[0].Expenses.ChildLivingCostYen);
        Assert.Equal(0, result.AnnualRows[1].Expenses.ChildLivingCostYen);
    }

    [Fact]
    public void Calculate_ExtendsChildLivingCostWhenGraduateSchoolIsSelected()
    {
        var input = CreateInput();
        input.Family.Children =
        [
            new ChildData { Age = 23 },
            new ChildData { Age = 23 }
        ];
        input.LifeEvents.EducationPlans =
        [
            new ChildEducationData { GraduateSchoolOptionValue = "graduate_public" },
            new ChildEducationData()
        ];

        var result = CalculateWithTestMaster(input);

        Assert.Equal(360_000, result.AnnualRows[0].Expenses.ChildLivingCostYen);
        Assert.Equal(0, result.AnnualRows[1].Expenses.ChildLivingCostYen);
    }

    private static LifePlanCalculationResult CalculateWithTestMaster(LifePlanData input)
    {
        var assumptions = CreateTestAssumptions(annualCostYen: 360_000, priceBaseYear: CurrentYear);

        return new LifePlanCalculator(assumptions).Calculate(input, CurrentYear);
    }

    private static SimulationAssumptions CreateTestAssumptions(long annualCostYen, int priceBaseYear)
    {
        return SimulationAssumptions.Current with
        {
            ChildLivingCosts = [new ChildLivingCostEntry(0, 23, annualCostYen, "test")],
            ChildLivingCostPriceBaseYear = priceBaseYear
        };
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
