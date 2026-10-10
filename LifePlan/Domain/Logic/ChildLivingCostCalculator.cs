using LifePlan.Domain.ReferenceData;

namespace LifePlan.Domain.Logic
{
    public static class ChildLivingCostCalculator
    {
        public static long CalculateAnnualCost(
            int? childAge,
            bool hasGraduateSchool,
            int year,
            SimulationAssumptions assumptions)
        {
            ArgumentNullException.ThrowIfNull(assumptions);

            var supportEndAge = hasGraduateSchool
                ? assumptions.ChildSupportEndAgeWithGraduateSchool
                : assumptions.ChildSupportEndAge;

            if (!childAge.HasValue || childAge.Value < 0 || childAge.Value >= supportEndAge)
            {
                return 0;
            }

            var entry = assumptions.ChildLivingCosts.FirstOrDefault(cost =>
                childAge.Value >= cost.StartAge &&
                childAge.Value <= cost.EndAge);

            if (entry is null)
            {
                return 0;
            }

            var annualCostYen = ApplyPriceChange(
                entry.AnnualCostYen,
                assumptions.InflationRatePercent,
                year - assumptions.ChildLivingCostPriceBaseYear);

            return decimal.ToInt64(decimal.Round(annualCostYen, 0, MidpointRounding.AwayFromZero));
        }

        private static decimal ApplyPriceChange(long baseAmountYen, decimal annualRatePercent, int years)
        {
            var amountYen = (decimal)baseAmountYen;
            var multiplier = 1m + annualRatePercent / 100m;

            for (var year = 0; year < Math.Abs(years); year++)
            {
                amountYen = years > 0 ? amountYen * multiplier : amountYen / multiplier;
            }

            return amountYen;
        }
    }
}
