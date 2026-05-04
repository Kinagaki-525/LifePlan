namespace LifePlan.ViewModels.LifePlan
{
    public class CashFlowTableRowViewModel
    {
        public string Category { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string CategoryKey { get; set; } = string.Empty;

        public IReadOnlyList<string> Values { get; set; } = [];
    }
}
