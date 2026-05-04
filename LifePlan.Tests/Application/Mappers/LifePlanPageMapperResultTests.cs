using LifePlan.Application.Mappers;
using LifePlan.Domain.Logic;

namespace LifePlan.Tests.Application.Mappers;

public class LifePlanPageMapperResultTests
{
    [Fact]
    public void ToResultViewModel_CreatesYearHeadersFromAnnualRows()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(2026),
            CreateAnnualRow(2027));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);

        Assert.Equal(["2026", "2027"], viewModel.YearHeaders);
    }

    [Fact]
    public void ToResultViewModel_ConvertsYenAmountsToManYenText()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(2026, husbandIncome: new PersonAnnualIncome(1_234_500, 0, 0)));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);
        var salaryRow = Assert.Single(viewModel.CashFlowRows, row => row.Label == "夫 給与");

        Assert.Equal("123.5", salaryRow.Values[0]);
    }

    [Fact]
    public void ToResultViewModel_ConvertsNullAgeToDash()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(2026, wifeAge: null));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);
        var wifeRow = Assert.Single(viewModel.CashFlowRows, row => row.Label == "妻");

        Assert.Equal("-", wifeRow.Values[0]);
    }

    [Fact]
    public void ToResultViewModel_ExcludesChildRowsWhenAllYearsAreEmpty()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(2026, childAges: [null, null]),
            CreateAnnualRow(2027, childAges: [null, null]));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);

        Assert.DoesNotContain(viewModel.CashFlowRows, row => row.Label == "第1子");
        Assert.DoesNotContain(viewModel.CashFlowRows, row => row.Label == "第2子");
    }

    [Fact]
    public void ToResultViewModel_SetsCategoryKeyForCashFlowRows()
    {
        var result = CreateCalculationResult(CreateAnnualRow(2026));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);

        Assert.Equal("family", Assert.Single(viewModel.CashFlowRows, row => row.Label == "夫").CategoryKey);
        Assert.Equal("income", Assert.Single(viewModel.CashFlowRows, row => row.Label == "収入合計").CategoryKey);
        Assert.Equal("expense", Assert.Single(viewModel.CashFlowRows, row => row.Label == "支出合計").CategoryKey);
        Assert.Equal("savings", Assert.Single(viewModel.CashFlowRows, row => row.Label == "開始時点金融資産").CategoryKey);
    }

    [Fact]
    public void ToResultViewModel_CreatesChartPointsInManYen()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(
                2026,
                husbandIncome: new PersonAnnualIncome(1_200_000, 300_000, 0),
                wifeIncome: new PersonAnnualIncome(800_000, 0, 200_000),
                expenses: new AnnualExpense(600_000, 120_000, 80_000, 40_000, 30_000, 20_000, 10_000, 50_000, 70_000),
                savingsBalanceWithoutReturnYen: 3_450_000,
                savingsBalanceWithReturnYen: 3_678_900));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);
        var point = Assert.Single(viewModel.ChartPoints);

        Assert.Equal(2026, point.Year);
        Assert.Equal(250m, point.TotalIncomeManYen);
        Assert.Equal(102m, point.TotalExpenseManYen);
        Assert.Equal(345m, point.SavingsBalanceWithoutReturnManYen);
        Assert.Equal(367.89m, point.SavingsBalanceWithReturnManYen);
    }

    private static LifePlanCalculationResult CreateCalculationResult(params AnnualCashFlowRow[] rows)
    {
        return new LifePlanCalculationResult(rows[0].Year, rows[^1].Year, rows);
    }

    private static AnnualCashFlowRow CreateAnnualRow(
        int year,
        int? husbandAge = 30,
        int? wifeAge = 30,
        IReadOnlyList<int?>? childAges = null,
        PersonAnnualIncome? husbandIncome = null,
        PersonAnnualIncome? wifeIncome = null,
        AnnualExpense? expenses = null,
        long savingsBalanceWithoutReturnYen = 0,
        long savingsBalanceWithReturnYen = 0)
    {
        return new AnnualCashFlowRow(
            year,
            husbandAge,
            wifeAge,
            childAges ?? [],
            husbandIncome ?? new PersonAnnualIncome(0, 0, 0),
            wifeIncome ?? new PersonAnnualIncome(0, 0, 0),
            expenses ?? new AnnualExpense(0, 0, 0, 0, 0, 0, 0, 0, 0),
            0,
            savingsBalanceWithoutReturnYen,
            savingsBalanceWithReturnYen);
    }
}
