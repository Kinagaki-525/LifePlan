namespace LifePlan.Domain.Rules
{
    public static class RateRules
    {
        public const decimal MinRatePercent = 0m;

        public const decimal MaxExpectedAnnualReturnRatePercent = 20m;

        public static bool IsExpectedAnnualReturnRateInRange(decimal ratePercent)
        {
            return ratePercent is >= MinRatePercent and <= MaxExpectedAnnualReturnRatePercent;
        }

    }
}
