namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanResultViewModel
    {
        public IReadOnlyList<string> YearHeaders { get; set; } = [];

        public IReadOnlyList<CashFlowTableRowViewModel> CashFlowRows { get; set; } = [];
    }
}
