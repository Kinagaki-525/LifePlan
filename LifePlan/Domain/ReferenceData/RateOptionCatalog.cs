namespace LifePlan.Domain.ReferenceData
{
    public static class RateOptionCatalog
    {
        public static IReadOnlyList<RateOptionEntry> AnnualIncomeChangeRates { get; } =
        [
            new(1m, "控えめ（年1%増）"),
            new(2m, "標準（年2%増）")
        ];

        public static IReadOnlyList<RateOptionEntry> InflationRates { get; } =
        [
            new(1m, "控えめ（年1%増）"),
            new(2m, "標準（年2%増）")
        ];
    }
}
