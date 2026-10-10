namespace LifePlan.Domain.Logic
{
    public record LifePlanCalculationResult(
        int StartYear,
        int EndYear,
        IReadOnlyList<AnnualCashFlowRow> AnnualRows,
        string AssumptionsVersion);

    public record AnnualCashFlowRow(
        int Year,
        int? HusbandAge,
        int? WifeAge,
        IReadOnlyList<int?> ChildAges,
        PersonAnnualIncome HusbandIncome,
        PersonAnnualIncome WifeIncome,
        AnnualExpense Expenses,
        long StartingAssetsYen,
        long SavingsBalanceWithoutReturnYen,
        long SavingsBalanceWithReturnYen)
    {
        public long TotalIncomeYen => HusbandIncome.TotalIncomeYen + WifeIncome.TotalIncomeYen;

        public long TotalExpenseYen => Expenses.TotalExpenseYen;

        public long AnnualBalanceYen => TotalIncomeYen - TotalExpenseYen;
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
        long TravelOtherYen,
        long ChildLivingCostYen,
        long HousingMaintenanceYen)
    {
        public long TotalExpenseYen =>
            BasicLivingCostYen +
            ChildLivingCostYen +
            HousingMaintenanceYen +
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
