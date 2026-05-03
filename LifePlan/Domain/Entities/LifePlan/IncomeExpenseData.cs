namespace LifePlan.Domain.Entities
{
    public class IncomeExpenseData
    {
        public PersonIncomeData HusbandIncome { get; set; } = new();

        public PersonIncomeData WifeIncome { get; set; } = new();

        public ExpenseData Expenses { get; set; } = new();
    }
}
