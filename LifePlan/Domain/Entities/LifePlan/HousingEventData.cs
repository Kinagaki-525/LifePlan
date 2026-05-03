namespace LifePlan.Domain.Entities
{
    public class HousingEventData
    {
        public long? DownPaymentYen { get; set; }

        public long? BorrowingAmountYen { get; set; }

        public int? PurchaseHusbandAge { get; set; }

        public int? LoanYears { get; set; }

        public decimal? InterestRatePercent { get; set; }
    }
}
