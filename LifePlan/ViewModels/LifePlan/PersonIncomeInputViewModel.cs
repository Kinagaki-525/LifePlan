namespace LifePlan.ViewModels.LifePlan
{
    public class PersonIncomeInputViewModel
    {
        public string? OccupationValue { get; set; }

        public decimal? AnnualIncomeManYen { get; set; }

        public decimal? AnnualIncomeChangeRatePercent { get; set; }

        public int? WorkStartAge { get; set; }

        public int? WorkEndAge { get; set; }

        public decimal? RetirementAllowanceManYen { get; set; }

        public decimal? AnnualPensionManYen { get; set; }

        public int? PensionStartAge { get; set; }
    }
}
