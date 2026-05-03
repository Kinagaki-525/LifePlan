namespace LifePlan.Domain.Rules
{
    public static class RateRules
    {
        public const decimal MinRatePercent = 0m;

        public const decimal MinAnnualIncomeChangeRatePercent = -100m;
        public const decimal MaxAnnualIncomeChangeRatePercent = 100m;

        public const decimal MaxExpectedAnnualReturnRatePercent = 20m;
        public const decimal MaxInflationRatePercent = 5m;

        public static bool IsExpectedAnnualReturnRateInRange(decimal ratePercent)
        {
            return ratePercent is >= MinRatePercent and <= MaxExpectedAnnualReturnRatePercent;
        }

        public static bool IsAnnualIncomeChangeRateInRange(decimal ratePercent)
        {
            return ratePercent is >= MinAnnualIncomeChangeRatePercent and <= MaxAnnualIncomeChangeRatePercent;
        }

        public static bool IsInflationRateInRange(decimal ratePercent)
        {
            return ratePercent is >= MinRatePercent and <= MaxInflationRatePercent;
        }
    }
}
