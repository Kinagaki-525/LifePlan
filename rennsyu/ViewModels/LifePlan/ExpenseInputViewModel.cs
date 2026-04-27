namespace rennsyu.ViewModels.LifePlan
{
    public class ExpenseInputViewModel
    {
        public decimal? MonthlyBasicLivingCostManYen { get; set; }

        public decimal? InflationRatePercent { get; set; }

        public decimal? MonthlyRentManYen { get; set; }

        public decimal? OtherAnnualCostManYen { get; set; }
    }
}
