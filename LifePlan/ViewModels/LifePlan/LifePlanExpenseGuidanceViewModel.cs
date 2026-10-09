namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanExpenseGuidanceViewModel
    {
        public string BasicLivingCostNote { get; set; } = string.Empty;

        public string ChildNote { get; set; } = string.Empty;

        public string HousingNote { get; set; } = string.Empty;

        public string OtherCostNote { get; set; } = string.Empty;

        public string InflationNote { get; set; } = string.Empty;

        public string? ChildMonthlyReference { get; set; }

        public IReadOnlyList<string> ChildMonthlyReferenceByAgeBand { get; set; } = [];

        public string ChildMonthlyReferenceCaveat { get; set; } = string.Empty;
    }
}
