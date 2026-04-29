using rennsyu.Domain.Entities;

namespace rennsyu.Domain.Logic
{
    public class LifePlanCalculator
    {
        private const int SimulationEndAge = 100;

        public LifePlanCalculationResult Calculate(LifePlanData input, int currentYear)
        {
            ArgumentNullException.ThrowIfNull(input);

            var yearsUntilHusbandTurns100 = SimulationEndAge - input.Family.HusbandAge;
            var yearsUntilWifeTurns100 = SimulationEndAge - input.Family.WifeAge;
            var simulationYears = Math.Max(yearsUntilHusbandTurns100, yearsUntilWifeTurns100);
            var annualRows = Enumerable.Range(0, simulationYears + 1)
                .Select(yearOffset => CreateAnnualRow(input, currentYear, yearOffset))
                .ToList();

            return new LifePlanCalculationResult(currentYear, currentYear + simulationYears, annualRows);
        }

        private static AnnualCashFlowRow CreateAnnualRow(
            LifePlanData input,
            int currentYear,
            int yearOffset)
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

            return new AnnualCashFlowRow(
                currentYear + yearOffset,
                husbandAge,
                wifeAge,
                childAges,
                husbandIncome,
                wifeIncome);
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

        private static long RoundToYen(decimal amountYen)
        {
            return decimal.ToInt64(decimal.Round(amountYen, 0, MidpointRounding.AwayFromZero));
        }
    }
}
