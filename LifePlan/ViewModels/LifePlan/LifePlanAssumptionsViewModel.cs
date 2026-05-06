namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanAssumptionsViewModel
    {
        public IReadOnlyList<string> GeneralNotes { get; set; } = [];

        public IReadOnlyList<EducationCostAssumptionViewModel> EducationCosts { get; set; } = [];
    }
}
