namespace LifePlan.Domain.Entities
{
    public class PersonIncomeData
    {
        public string? OccupationValue { get; set; }

        public long? AnnualIncomeYen { get; set; }

        public decimal? AnnualIncomeChangeRatePercent { get; set; }

        public int? WorkStartAge { get; set; }

        public int? WorkEndAge { get; set; }

        public long? RetirementAllowanceYen { get; set; }

        public long? AnnualPensionYen { get; set; }

        public int? PensionStartAge { get; set; }
    }
}
