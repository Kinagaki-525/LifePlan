namespace LifePlan.Domain.Logic
{
    public record LifePlanCalculationResult(
        int StartYear,
        int EndYear,
        IReadOnlyList<AnnualCashFlowRow> AnnualRows);

    public record AnnualCashFlowRow(
        int Year,
        int? HusbandAge,
        int? WifeAge,
        IReadOnlyList<int?> ChildAges,
        PersonAnnualIncome HusbandIncome,
        PersonAnnualIncome WifeIncome)
    {
        public long TotalIncomeYen => HusbandIncome.TotalIncomeYen + WifeIncome.TotalIncomeYen;
    }

    public record PersonAnnualIncome(
        long SalaryYen,
        long RetirementAllowanceYen,
        long PensionYen)
    {
        public long TotalIncomeYen => SalaryYen + RetirementAllowanceYen + PensionYen;
    }
}
