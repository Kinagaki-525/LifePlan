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
        PersonAnnualIncome WifeIncome,
        AnnualExpense Expenses)
    {
        public long TotalIncomeYen => HusbandIncome.TotalIncomeYen + WifeIncome.TotalIncomeYen;

        public long TotalExpenseYen => Expenses.TotalExpenseYen;
    }

    public record PersonAnnualIncome(
        long SalaryYen,
        long RetirementAllowanceYen,
        long PensionYen)
    {
        public long TotalIncomeYen => SalaryYen + RetirementAllowanceYen + PensionYen;
    }

    public record AnnualExpense(
        long BasicLivingCostYen,
        long RentYen,
        long OtherAnnualCostYen,
        long MarriageYen,
        long HousingDownPaymentYen,
        long HousingLoanRepaymentYen,
        long CarYen,
        long EducationYen,
        long TravelOtherYen)
    {
        public long TotalExpenseYen =>
            BasicLivingCostYen +
            RentYen +
            OtherAnnualCostYen +
            MarriageYen +
            HousingDownPaymentYen +
            HousingLoanRepaymentYen +
            CarYen +
            EducationYen +
            TravelOtherYen;
    }
}
