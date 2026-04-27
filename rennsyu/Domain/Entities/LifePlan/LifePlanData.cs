namespace rennsyu.Domain.Entities
{
    public class LifePlanData
    {
        public FamilyData Family { get; set; } = new();

        public LifeEventData LifeEvents { get; set; } = new();

        public AssetData Assets { get; set; } = new();

        public IncomeExpenseData IncomeExpense { get; set; } = new();
    }
}
