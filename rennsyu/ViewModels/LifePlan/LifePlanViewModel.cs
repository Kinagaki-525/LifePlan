namespace rennsyu.ViewModels.LifePlan
{
    public class LifePlanViewModel
    {
        public FamilyInputViewModel Family { get; set; } = new();

        public LifeEventInputViewModel LifeEvents { get; set; } = new();

        public SavingsInputViewModel Savings { get; set; } = new();

        public IncomeExpenseInputViewModel IncomeExpense { get; set; } = new();

        public bool IsSubmitted { get; set; }

        public IReadOnlyList<SelectOptionViewModel> ChildAgeOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> EducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> NurseryEducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> KindergartenEducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> ElementarySchoolEducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> JuniorHighSchoolEducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> HighSchoolEducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> UniversityEducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> GraduateSchoolEducationOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> PensionReferenceOptions { get; set; } = [];

        public IReadOnlyList<SelectOptionViewModel> OccupationOptions { get; set; } = [];
    }
}
