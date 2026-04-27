namespace rennsyu.ViewModels.LifePlan
{
    public class LifePlanViewModel
    {
        public FamilyInputViewModel Family { get; set; } = new();

        public LifeEventInputViewModel LifeEvents { get; set; } = new();

        public SavingsInputViewModel Savings { get; set; } = new();

        public IncomeExpenseInputViewModel IncomeExpense { get; set; } = new();

        public IReadOnlyList<SelectOptionViewModel> ChildAgeOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> EducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> PensionReferenceOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> OccupationOptions { get; set; } = [];
    }
}
