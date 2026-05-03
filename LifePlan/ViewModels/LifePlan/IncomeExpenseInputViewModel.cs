namespace LifePlan.ViewModels.LifePlan
{
    public class IncomeExpenseInputViewModel
    {
        public PersonIncomeInputViewModel HusbandIncome { get; set; } = new();

        public PersonIncomeInputViewModel WifeIncome { get; set; } = new();

        public ExpenseInputViewModel Expenses { get; set; } = new();
    }
}
