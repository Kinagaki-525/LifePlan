namespace LifePlan.Domain.Rules
{
    public static class RateRules
    {
        public const decimal MinRatePercent = 0m;

        public const decimal SmallAnnualIncomeChangeRatePercent = 1m;
        public const decimal LargeAnnualIncomeChangeRatePercent = 2m;

        public const decimal MaxExpectedAnnualReturnRatePercent = 20m;
        public const decimal MaxInflationRatePercent = 5m;

        public static bool IsExpectedAnnualReturnRateInRange(decimal ratePercent)
        {
            return ratePercent is >= MinRatePercent and <= MaxExpectedAnnualReturnRatePercent;
        }

        public static bool IsAnnualIncomeChangeRateDefined(decimal ratePercent)
        {
            return ratePercent is SmallAnnualIncomeChangeRatePercent or LargeAnnualIncomeChangeRatePercent;
        }

        public static bool IsInflationRateInRange(decimal ratePercent)
        {
            return ratePercent is >= MinRatePercent and <= MaxInflationRatePercent;
        }
    }
}
