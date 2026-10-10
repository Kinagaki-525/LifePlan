using LifePlan.Domain.Entities;
using LifePlan.Domain.Logic;

namespace LifePlan.Tests.Domain.Logic;

public class LifePlanCalculatorExpenseTests
{
    private const int CurrentYear = 2026;

    [Fact]
    public void Calculate_AppliesFixedInflationToBasicLivingCost()
    {
        var input = CreateInput();
        input.IncomeExpense.Expenses.MonthlyBasicLivingCostYen = 100_000;

        var result = Calculate(input);

        Assert.Equal(1_200_000, result.AnnualRows[0].Expenses.BasicLivingCostYen);
        Assert.Equal(1_224_000, result.AnnualRows[1].Expenses.BasicLivingCostYen);
    }

    [Fact]
    public void Calculate_AppliesFixedInflationToOtherAnnualCost()
    {
        var input = CreateInput();
        input.IncomeExpense.Expenses.OtherAnnualCostYen = 2_400_000;

        var result = Calculate(input);

        Assert.Equal(2_400_000, result.AnnualRows[0].Expenses.OtherAnnualCostYen);
        Assert.Equal(2_448_000, result.AnnualRows[1].Expenses.OtherAnnualCostYen);
    }

    [Fact]
    public void Calculate_AppliesFixedInflationToEducationCostFromSecondYear()
    {
        var input = CreateInput();
        input.Family.Children =
        [
            new ChildData { Age = 5 }
        ];
        input.LifeEvents.EducationPlans =
        [
            new ChildEducationData { ElementarySchoolOptionValue = "elementary_public" }
        ];

        var result = Calculate(input);

        Assert.Equal(0, result.AnnualRows[0].Expenses.EducationYen);
        Assert.Equal(408_000, result.AnnualRows[1].Expenses.EducationYen);
    }

    [Fact]
    public void Calculate_DoesNotLinkSalaryToInflation()
    {
        var input = CreateInput();
        input.IncomeExpense.HusbandIncome = new PersonIncomeData
        {
            AnnualIncomeYen = 5_000_000,
            WorkStartAge = 30,
            WorkEndAge = 31
        };

        var result = Calculate(input);

        Assert.Equal(5_000_000, result.AnnualRows[0].HusbandIncome.SalaryYen);
        Assert.Equal(5_000_000, result.AnnualRows[1].HusbandIncome.SalaryYen);
    }

    [Fact]
    public void Calculate_DoesNotApplyInflationToRentHousingCarAndMarriage()
    {
        var input = CreateInput();
        input.IncomeExpense.Expenses.MonthlyRentYen = 80_000;
        input.LifeEvents.Marriage = new MarriageEventData
        {
            HusbandAge = 31,
            CostYen = 2_000_000
        };
        input.LifeEvents.Car = new CarEventData
        {
            FirstPurchaseHusbandAge = 31,
            ReplacementIntervalYears = 5,
            PurchaseAmountYen = 1_500_000
        };
        input.LifeEvents.Housing = new HousingEventData
        {
            PurchaseHusbandAge = 32,
            DownPaymentYen = 1_000_000,
            BorrowingAmountYen = 3_000_000,
            LoanYears = 3,
            InterestRatePercent = 0m
        };

        var result = Calculate(input);

        Assert.Equal(960_000, result.AnnualRows[1].Expenses.RentYen);
        Assert.Equal(2_000_000, result.AnnualRows[1].Expenses.MarriageYen);
        Assert.Equal(1_500_000, result.AnnualRows[1].Expenses.CarYen);
        Assert.Equal(1_000_000, result.AnnualRows[2].Expenses.HousingDownPaymentYen);
        Assert.Equal(1_000_000, result.AnnualRows[2].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(1_000_000, result.AnnualRows[3].Expenses.HousingLoanRepaymentYen);
    }

    [Fact]
    public void Calculate_StopsRentFromHousingPurchaseYear()
    {
        var input = CreateInput();
        input.IncomeExpense.Expenses.MonthlyRentYen = 80_000;
        input.LifeEvents.Housing.PurchaseHusbandAge = 31;

        var result = Calculate(input);

        Assert.Equal(960_000, result.AnnualRows[0].Expenses.RentYen);
        Assert.Equal(0, result.AnnualRows[1].Expenses.RentYen);
    }

    [Fact]
    public void Calculate_AddsHousingDownPaymentAndZeroInterestLoanRepayments()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = new HousingEventData
        {
            PurchaseHusbandAge = 30,
            DownPaymentYen = 3_000_000,
            BorrowingAmountYen = 12_000_000,
            LoanYears = 3,
            InterestRatePercent = 0m
        };

        var result = Calculate(input);

        Assert.Equal(3_000_000, result.AnnualRows[0].Expenses.HousingDownPaymentYen);
        Assert.Equal(4_000_000, result.AnnualRows[0].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(4_000_000, result.AnnualRows[1].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(4_000_000, result.AnnualRows[2].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(0, result.AnnualRows[3].Expenses.HousingLoanRepaymentYen);
    }

    [Fact]
    public void Calculate_DoesNotAddHousingLoanBeforePurchaseOrAfterLoanTerm()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = new HousingEventData
        {
            PurchaseHusbandAge = 32,
            DownPaymentYen = 3_000_000,
            BorrowingAmountYen = 12_000_000,
            LoanYears = 2,
            InterestRatePercent = 0m
        };

        var result = Calculate(input);

        Assert.Equal(0, result.AnnualRows[1].Expenses.HousingDownPaymentYen);
        Assert.Equal(0, result.AnnualRows[1].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(3_000_000, result.AnnualRows[2].Expenses.HousingDownPaymentYen);
        Assert.Equal(6_000_000, result.AnnualRows[2].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(6_000_000, result.AnnualRows[3].Expenses.HousingLoanRepaymentYen);
        Assert.Equal(0, result.AnnualRows[4].Expenses.HousingLoanRepaymentYen);
    }

    [Fact]
    public void Calculate_DoesNotAddBorrowingAmountDirectlyToExpenseTotal()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = new HousingEventData
        {
            PurchaseHusbandAge = 30,
            DownPaymentYen = 3_000_000,
            BorrowingAmountYen = 12_000_000,
            LoanYears = 3,
            InterestRatePercent = 0m
        };

        var result = Calculate(input);

        Assert.Equal(150_000, result.AnnualRows[0].Expenses.HousingMaintenanceYen);
        Assert.Equal(7_150_000, result.AnnualRows[0].TotalExpenseYen);
    }

    [Fact]
    public void Calculate_RoundsInterestBearingLoanRepaymentToYen()
    {
        var input = CreateInput();
        input.LifeEvents.Housing = new HousingEventData
        {
            PurchaseHusbandAge = 30,
            BorrowingAmountYen = 12_000_000,
            LoanYears = 10,
            InterestRatePercent = 2m
        };

        var result = Calculate(input);

        Assert.Equal(1_335_918, result.AnnualRows[0].Expenses.HousingLoanRepaymentYen);
    }

    [Fact]
    public void Calculate_AddsHusbandAgeBasedEventExpenses()
    {
        var input = CreateInput();
        input.LifeEvents.Marriage = new MarriageEventData
        {
            HusbandAge = 30,
            CostYen = 2_000_000
        };
        input.LifeEvents.Car = new CarEventData
        {
            FirstPurchaseHusbandAge = 31,
            ReplacementIntervalYears = 2,
            PurchaseAmountYen = 1_500_000
        };
        input.LifeEvents.TravelOther = new TravelOtherEventData
        {
            StartHusbandAge = 30,
            EndHusbandAge = 31,
            AnnualCostYen = 300_000
        };

        var result = Calculate(input);

        Assert.Equal(2_000_000, result.AnnualRows[0].Expenses.MarriageYen);
        Assert.Equal(300_000, result.AnnualRows[0].Expenses.TravelOtherYen);
        Assert.Equal(1_500_000, result.AnnualRows[1].Expenses.CarYen);
        Assert.Equal(306_000, result.AnnualRows[1].Expenses.TravelOtherYen);
        Assert.Equal(0, result.AnnualRows[2].Expenses.CarYen);
        Assert.Equal(1_500_000, result.AnnualRows[3].Expenses.CarYen);
    }

    [Fact]
    public void Calculate_AddsTravelOtherCostFromStartThroughEndAge()
    {
        var input = CreateInput();
        input.LifeEvents.TravelOther = new TravelOtherEventData
        {
            StartHusbandAge = 31,
            EndHusbandAge = 32,
            AnnualCostYen = 300_000
        };

        var result = Calculate(input);

        Assert.Equal(0, result.AnnualRows[0].Expenses.TravelOtherYen);
        Assert.Equal(306_000, result.AnnualRows[1].Expenses.TravelOtherYen);
        Assert.Equal(312_120, result.AnnualRows[2].Expenses.TravelOtherYen);
        Assert.Equal(0, result.AnnualRows[3].Expenses.TravelOtherYen);
    }

    [Fact]
    public void Calculate_AddsCarCostOnlyOnFirstPurchaseAndReplacementYears()
    {
        var input = CreateInput();
        input.LifeEvents.Car = new CarEventData
        {
            FirstPurchaseHusbandAge = 32,
            ReplacementIntervalYears = 3,
            PurchaseAmountYen = 1_500_000
        };

        var result = Calculate(input);

        Assert.Equal(0, result.AnnualRows[1].Expenses.CarYen);
        Assert.Equal(1_500_000, result.AnnualRows[2].Expenses.CarYen);
        Assert.Equal(0, result.AnnualRows[3].Expenses.CarYen);
        Assert.Equal(0, result.AnnualRows[4].Expenses.CarYen);
        Assert.Equal(1_500_000, result.AnnualRows[5].Expenses.CarYen);
    }

    [Fact]
    public void Calculate_AddsEducationCostsUsingFirstAndLaterYearMasterValues()
    {
        var input = CreateInput();
        input.Family.Children =
        [
            new ChildData { Age = 18 }
        ];
        input.LifeEvents.EducationPlans =
        [
            new ChildEducationData { UniversityOptionValue = "university_public" }
        ];

        var result = Calculate(input);

        Assert.Equal(850_000, result.AnnualRows[0].Expenses.EducationYen);
        Assert.Equal(561_000, result.AnnualRows[1].Expenses.EducationYen);
    }

    [Fact]
    public void Calculate_AddsEducationCostWhenNegativeChildAgeReachesTargetAge()
    {
        var input = CreateInput();
        input.Family.Children =
        [
            new ChildData { Age = -1 }
        ];
        input.LifeEvents.EducationPlans =
        [
            new ChildEducationData { NurseryOptionValue = "nursery_public" }
        ];

        var result = Calculate(input);

        Assert.Equal(0, result.AnnualRows[0].Expenses.EducationYen);
        Assert.Equal(459_000, result.AnnualRows[1].Expenses.EducationYen);
    }

    [Fact]
    public void Calculate_DoesNotAddEducationCostForChildWithoutAge()
    {
        var input = CreateInput();
        input.Family.Children =
        [
            new ChildData { Age = null }
        ];
        input.LifeEvents.EducationPlans =
        [
            new ChildEducationData { NurseryOptionValue = "nursery_public" }
        ];

        var result = Calculate(input);

        Assert.Equal(0, result.AnnualRows[0].Expenses.EducationYen);
    }

    [Fact]
    public void Calculate_SumsEducationCostsForMultipleChildren()
    {
        var input = CreateInput();
        input.Family.Children =
        [
            new ChildData { Age = 0 },
            new ChildData { Age = 18 }
        ];
        input.LifeEvents.EducationPlans =
        [
            new ChildEducationData { NurseryOptionValue = "nursery_public" },
            new ChildEducationData { UniversityOptionValue = "university_public" }
        ];

        var result = Calculate(input);

        Assert.Equal(1_300_000, result.AnnualRows[0].Expenses.EducationYen);
    }

    [Fact]
    public void Calculate_SumsMultipleExpenseCategories()
    {
        var input = CreateInput();
        input.IncomeExpense.Expenses.MonthlyBasicLivingCostYen = 100_000;
        input.IncomeExpense.Expenses.MonthlyRentYen = 50_000;
        input.IncomeExpense.Expenses.OtherAnnualCostYen = 100_000;
        input.LifeEvents.Marriage = new MarriageEventData
        {
            HusbandAge = 30,
            CostYen = 200_000
        };

        var result = Calculate(input);

        Assert.Equal(2_100_000, result.AnnualRows[0].Expenses.TotalExpenseYen);
        Assert.Equal(2_100_000, result.AnnualRows[0].TotalExpenseYen);
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
