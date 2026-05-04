namespace LifePlan.ViewModels.LifePlan
{
    public class EducationCostAssumptionViewModel
    {
        public string Stage { get; set; } = string.Empty;

        public IReadOnlyList<string> CostLines { get; set; } = [];
    }
}
