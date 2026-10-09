namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanResultViewModel
    {
        public LifePlanFirstYearSummaryViewModel FirstYearSummary { get; set; } = new();

        public IReadOnlyList<AutoCostSummaryViewModel> AutoCosts { get; set; } = [];

        public IReadOnlyList<string> YearHeaders { get; set; } = [];

        public IReadOnlyList<CashFlowTableRowViewModel> CashFlowRows { get; set; } = [];

        public IReadOnlyList<LifePlanChartPointViewModel> ChartPoints { get; set; } = [];

        public LifePlanAssumptionsViewModel Assumptions { get; set; } = new();
    }
}
