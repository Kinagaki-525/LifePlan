namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanChartPointViewModel
    {
        public int Year { get; set; }

        public decimal TotalIncomeManYen { get; set; }

        public decimal TotalExpenseManYen { get; set; }

        public decimal SavingsBalanceWithoutReturnManYen { get; set; }

        public decimal SavingsBalanceWithReturnManYen { get; set; }
    }
}
