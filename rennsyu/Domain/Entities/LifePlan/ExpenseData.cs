namespace rennsyu.Domain.Entities
{
    public class ExpenseData
    {
        public long? MonthlyBasicLivingCostYen { get; set; }

        public decimal? InflationRatePercent { get; set; }

        public long? MonthlyRentYen { get; set; }

        public long? OtherAnnualCostYen { get; set; }
    }
}
