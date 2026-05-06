using LifePlan.Domain.Entities;
using LifePlan.Domain.ReferenceData;

namespace LifePlan.Domain.Logic
{
    public class LifePlanCalculator
    {
        private const int SimulationEndAge = 100;
        private const long ManYenUnit = 10000;

        public LifePlanCalculationResult Calculate(LifePlanData input, int currentYear)
        {
            ArgumentNullException.ThrowIfNull(input);

            var yearsUntilHusbandTurns100 = SimulationEndAge - input.Family.HusbandAge;
            var yearsUntilWifeTurns100 = SimulationEndAge - input.Family.WifeAge;
            var simulationYears = Math.Max(yearsUntilHusbandTurns100, yearsUntilWifeTurns100);
            var annualRows = CreateAnnualRows(input, currentYear, simulationYears);

            return new LifePlanCalculationResult(currentYear, currentYear + simulationYears, annualRows);
        }

        private static List<AnnualCashFlowRow> CreateAnnualRows(
            LifePlanData input,
            int currentYear,
            int simulationYears)
        {
            var annualRows = new List<AnnualCashFlowRow>();
            var startingAssetsYen = input.Assets.CurrentFinancialAssetsYen.GetValueOrDefault();
            var previousBalanceWithReturnYen = startingAssetsYen;

            for (var yearOffset = 0; yearOffset <= simulationYears; yearOffset++)
            {
                var row = CreateAnnualRow(
                    input,
                    currentYear,
                    yearOffset,
                    startingAssetsYen,
                    previousBalanceWithReturnYen);

                annualRows.Add(row);
                startingAssetsYen = row.SavingsBalanceWithoutReturnYen;
                previousBalanceWithReturnYen = row.SavingsBalanceWithReturnYen;
            }

            return annualRows;
        }

        private static AnnualCashFlowRow CreateAnnualRow(
            LifePlanData input,
            int currentYear,
            int yearOffset,
            long startingAssetsYen,
            long previousBalanceWithReturnYen)
        {
            var husbandAge = CalculateAdultAge(input.Family.HusbandAge, yearOffset);
            var wifeAge = CalculateAdultAge(input.Family.WifeAge, yearOffset);
            var childAges = input.Family.Children
                .Select(child => CalculateChildAge(child.Age, yearOffset))
                .ToList();
            var husbandIncome = CalculatePersonIncome(
                input.IncomeExpense.HusbandIncome,
                husbandAge,
                yearOffset);
            var wifeIncome = CalculatePersonIncome(
                input.IncomeExpense.WifeIncome,
                wifeAge,
                yearOffset);
            var expenses = CalculateExpenses(input, yearOffset);

            return new AnnualCashFlowRow(
                currentYear + yearOffset,
                husbandAge,
                wifeAge,
                childAges,
                husbandIncome,
                wifeIncome,
                expenses,
                startingAssetsYen,
                CalculateBalanceWithoutReturn(startingAssetsYen, husbandIncome, wifeIncome, expenses),
                CalculateBalanceWithReturn(input, previousBalanceWithReturnYen, husbandIncome, wifeIncome, expenses));
        }

        private static long CalculateBalanceWithoutReturn(
            long startingAssetsYen,
            PersonAnnualIncome husbandIncome,
            PersonAnnualIncome wifeIncome,
            AnnualExpense expenses)
        {
            return startingAssetsYen + CalculateAnnualBalance(husbandIncome, wifeIncome, expenses);
        }

        private static long CalculateBalanceWithReturn(
            LifePlanData input,
            long previousBalanceWithReturnYen,
            PersonAnnualIncome husbandIncome,
            PersonAnnualIncome wifeIncome,
            AnnualExpense expenses)
        {
            var assetsAfterReturnYen = ApplyAnnualReturn(
                previousBalanceWithReturnYen,
                input.Assets.ExpectedAnnualReturnRatePercent.GetValueOrDefault());

            return assetsAfterReturnYen + CalculateAnnualBalance(husbandIncome, wifeIncome, expenses);
        }

        private static long CalculateAnnualBalance(
            PersonAnnualIncome husbandIncome,
            PersonAnnualIncome wifeIncome,
            AnnualExpense expenses)
        {
            return husbandIncome.TotalIncomeYen + wifeIncome.TotalIncomeYen - expenses.TotalExpenseYen;
        }

        private static int? CalculateAdultAge(int initialAge, int yearOffset)
        {
            var age = initialAge + yearOffset;

            return age <= SimulationEndAge ? age : null;
        }

        private static int? CalculateChildAge(int? initialAge, int yearOffset)
        {
            if (!initialAge.HasValue)
            {
                return null;
            }

            var age = initialAge.Value + yearOffset;

            return age >= 0 ? age : null;
        }

        private static PersonAnnualIncome CalculatePersonIncome(
            PersonIncomeData income,
            int? age,
            int yearOffset)
        {
            if (!age.HasValue)
            {
                return new PersonAnnualIncome(0, 0, 0);
            }

            var salaryYen = CalculateSalary(income, age.Value, yearOffset);
            var retirementAllowanceYen = CalculateRetirementAllowance(income, age.Value);
            var pensionYen = CalculatePension(income, age.Value);

            return new PersonAnnualIncome(salaryYen, retirementAllowanceYen, pensionYen);
        }

        private static long CalculateSalary(PersonIncomeData income, int age, int yearOffset)
        {
            if (!income.AnnualIncomeYen.HasValue ||
                !income.WorkStartAge.HasValue ||
                !income.WorkEndAge.HasValue ||
                age < income.WorkStartAge.Value ||
                age > income.WorkEndAge.Value)
            {
                return 0;
            }

            var salaryYen = ApplyAnnualChange(
                income.AnnualIncomeYen.Value,
                income.AnnualIncomeChangeRatePercent.GetValueOrDefault(),
                yearOffset);

            return RoundToYen(salaryYen);
        }

        private static long CalculateRetirementAllowance(PersonIncomeData income, int age)
        {
            if (!income.WorkEndAge.HasValue || age != income.WorkEndAge.Value)
            {
                return 0;
            }

            return income.RetirementAllowanceYen.GetValueOrDefault();
        }

        private static long CalculatePension(PersonIncomeData income, int age)
        {
            if (!income.PensionStartAge.HasValue || age < income.PensionStartAge.Value)
            {
                return 0;
            }

            return income.AnnualPensionYen.GetValueOrDefault();
        }

        private static AnnualExpense CalculateExpenses(LifePlanData input, int yearOffset)
        {
            var rawHusbandAge = input.Family.HusbandAge + yearOffset;
            var expenses = input.IncomeExpense.Expenses;

            return new AnnualExpense(
                CalculateBasicLivingCost(expenses, yearOffset),
                CalculateRent(input, rawHusbandAge),
                expenses.OtherAnnualCostYen.GetValueOrDefault(),
                CalculateMarriageCost(input.LifeEvents.Marriage, rawHusbandAge),
                CalculateHousingDownPayment(input.LifeEvents.Housing, rawHusbandAge),
                CalculateHousingLoanRepayment(input.LifeEvents.Housing, rawHusbandAge),
                CalculateCarCost(input.LifeEvents.Car, rawHusbandAge),
                CalculateEducationCost(input, yearOffset),
                CalculateTravelOtherCost(input.LifeEvents.TravelOther, rawHusbandAge));
        }

        private static long CalculateBasicLivingCost(ExpenseData expenses, int yearOffset)
        {
            var annualLivingCostYen = expenses.MonthlyBasicLivingCostYen.GetValueOrDefault() * 12;
            var adjustedLivingCostYen = ApplyAnnualChange(
                annualLivingCostYen,
                expenses.InflationRatePercent.GetValueOrDefault(),
                yearOffset);

            return RoundToYen(adjustedLivingCostYen);
        }

        private static long CalculateRent(LifePlanData input, int rawHusbandAge)
        {
            var purchaseHusbandAge = input.LifeEvents.Housing.PurchaseHusbandAge;
            if (purchaseHusbandAge.HasValue && rawHusbandAge >= purchaseHusbandAge.Value)
            {
                return 0;
            }

            return input.IncomeExpense.Expenses.MonthlyRentYen.GetValueOrDefault() * 12;
        }

        private static long CalculateMarriageCost(MarriageEventData marriage, int rawHusbandAge)
        {
            if (!marriage.HusbandAge.HasValue || rawHusbandAge != marriage.HusbandAge.Value)
            {
                return 0;
            }

            return marriage.CostYen.GetValueOrDefault();
        }

        private static long CalculateHousingDownPayment(HousingEventData housing, int rawHusbandAge)
        {
            if (!housing.PurchaseHusbandAge.HasValue || rawHusbandAge != housing.PurchaseHusbandAge.Value)
            {
                return 0;
            }

            return housing.DownPaymentYen.GetValueOrDefault();
        }

        private static long CalculateHousingLoanRepayment(HousingEventData housing, int rawHusbandAge)
        {
            if (!housing.PurchaseHusbandAge.HasValue ||
                !housing.LoanYears.HasValue ||
                housing.LoanYears.Value <= 0)
            {
                return 0;
            }

            var yearsSincePurchase = rawHusbandAge - housing.PurchaseHusbandAge.Value;
            if (yearsSincePurchase < 0 || yearsSincePurchase >= housing.LoanYears.Value)
            {
                return 0;
            }

            return CalculateAnnualLoanRepayment(
                housing.BorrowingAmountYen.GetValueOrDefault(),
                housing.LoanYears.Value,
                housing.InterestRatePercent.GetValueOrDefault());
        }

        private static long CalculateAnnualLoanRepayment(
            long borrowingAmountYen,
            int loanYears,
            decimal interestRatePercent)
        {
            if (borrowingAmountYen == 0 || loanYears <= 0)
            {
                return 0;
            }

            if (interestRatePercent == 0)
            {
                return RoundToYen((decimal)borrowingAmountYen / loanYears);
            }

            var annualRate = interestRatePercent / 100m;
            var compoundRate = CalculateCompoundRate(annualRate, loanYears);
            var annualRepaymentYen = borrowingAmountYen * annualRate * compoundRate / (compoundRate - 1m);

            return RoundToYen(annualRepaymentYen);
        }

        private static decimal CalculateCompoundRate(decimal annualRate, int years)
        {
            var compoundRate = 1m;

            for (var year = 0; year < years; year++)
            {
                compoundRate *= 1m + annualRate;
            }

            return compoundRate;
        }

        private static long CalculateCarCost(CarEventData car, int rawHusbandAge)
        {
            if (!car.FirstPurchaseHusbandAge.HasValue ||
                !car.ReplacementIntervalYears.HasValue ||
                car.ReplacementIntervalYears.Value <= 0 ||
                rawHusbandAge < car.FirstPurchaseHusbandAge.Value)
            {
                return 0;
            }

            var yearsSinceFirstPurchase = rawHusbandAge - car.FirstPurchaseHusbandAge.Value;
            if (yearsSinceFirstPurchase % car.ReplacementIntervalYears.Value != 0)
            {
                return 0;
            }

            return car.PurchaseAmountYen.GetValueOrDefault();
        }

        private static long CalculateEducationCost(LifePlanData input, int yearOffset)
        {
            var totalCostYen = 0L;

            for (var childIndex = 0; childIndex < input.Family.Children.Count; childIndex++)
            {
                if (childIndex >= input.LifeEvents.EducationPlans.Count)
                {
                    continue;
                }

                var childInitialAge = input.Family.Children[childIndex].Age;
                if (!childInitialAge.HasValue)
                {
                    continue;
                }

                var childAge = childInitialAge.Value + yearOffset;
                totalCostYen += CalculateChildEducationCost(
                    input.LifeEvents.EducationPlans[childIndex],
                    childAge);
            }

            return totalCostYen;
        }

        private static long CalculateChildEducationCost(ChildEducationData education, int childAge)
        {
            if (childAge < 0)
            {
                return 0;
            }

            var optionValues = new[]
            {
                education.NurseryOptionValue,
                education.KindergartenOptionValue,
                education.ElementarySchoolOptionValue,
                education.JuniorHighSchoolOptionValue,
                education.HighSchoolOptionValue,
                education.UniversityOptionValue,
                education.GraduateSchoolOptionValue
            };

            return optionValues
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => EducationCostMaster.Entries.FirstOrDefault(entry =>
                    entry.Value == value &&
                    childAge >= entry.StartAge &&
                    childAge <= entry.EndAge))
                .Where(entry => entry is not null)
                .Sum(entry => ToEducationCostYen(entry!, childAge));
        }

        private static long ToEducationCostYen(EducationCostEntry entry, int childAge)
        {
            var costManYen = childAge == entry.StartAge
                ? entry.FirstYearCostManYen
                : entry.LaterYearCostManYen;

            return RoundToYen(costManYen * ManYenUnit);
        }

        private static long CalculateTravelOtherCost(TravelOtherEventData travelOther, int rawHusbandAge)
        {
            if (!travelOther.StartHusbandAge.HasValue ||
                !travelOther.EndHusbandAge.HasValue ||
                rawHusbandAge < travelOther.StartHusbandAge.Value ||
                rawHusbandAge > travelOther.EndHusbandAge.Value)
            {
                return 0;
            }

            return travelOther.AnnualCostYen.GetValueOrDefault();
        }

        private static decimal ApplyAnnualChange(
            long baseAmountYen,
            decimal annualChangeRatePercent,
            int yearOffset)
        {
            var amountYen = (decimal)baseAmountYen;
            var multiplier = 1m + annualChangeRatePercent / 100m;

            for (var year = 0; year < yearOffset; year++)
            {
                amountYen *= multiplier;
            }

            return amountYen;
        }

        private static long ApplyAnnualReturn(long balanceYen, decimal annualReturnRatePercent)
        {
            return RoundToYen(balanceYen * (1m + annualReturnRatePercent / 100m));
        }

        private static long RoundToYen(decimal amountYen)
        {
            return decimal.ToInt64(decimal.Round(amountYen, 0, MidpointRounding.AwayFromZero));
        }
    }
}
