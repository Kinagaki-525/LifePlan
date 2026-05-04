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
        AnnualExpense? expenses = null)
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
            0,
            0);
    }
}
