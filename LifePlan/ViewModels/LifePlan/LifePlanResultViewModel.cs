namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanResultViewModel
    {
        public IReadOnlyList<string> YearHeaders { get; set; } = [];

        public IReadOnlyList<CashFlowTableRowViewModel> CashFlowRows { get; set; } = [];

        public IReadOnlyList<LifePlanChartPointViewModel> ChartPoints { get; set; } = [];

        public LifePlanAssumptionsViewModel Assumptions { get; set; } = new();
    }
}
