namespace LifePlan.Domain.ReferenceData
{
    public record SimulationAssumptions(
        string Version,
        decimal InflationRatePercent,
        decimal HousingMaintenanceRatePercent,
        int ChildSupportEndAge,
        int ChildSupportEndAgeWithGraduateSchool,
        IReadOnlyList<ChildLivingCostEntry> ChildLivingCosts,
        int ChildLivingCostPriceBaseYear,
        IReadOnlyList<string> SupportedCalculationSpecVersions,
        string CurrentCalculationSpecVersion)
    {
        public static SimulationAssumptions Current { get; } = new(
            Version: "2026.10",
            InflationRatePercent: 2m,
            HousingMaintenanceRatePercent: 1m,
            ChildSupportEndAge: 22,
            ChildSupportEndAgeWithGraduateSchool: 24,
            ChildLivingCosts: ChildLivingCostMaster.Entries,
            ChildLivingCostPriceBaseYear: ChildLivingCostMaster.PriceBaseYear,
            SupportedCalculationSpecVersions: ["2"],
            CurrentCalculationSpecVersion: "2");
    }
}
