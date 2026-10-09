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
    public void ToResultViewModel_AddsChildLivingCostRowAfterBasicLivingCost()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(
                2026,
                expenses: new AnnualExpense(0, 0, 0, 0, 0, 0, 0, 0, 0, 613_836, 0)));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);
        var labels = viewModel.CashFlowRows.Select(row => row.Label).ToList();
        var childLivingCostRow = Assert.Single(viewModel.CashFlowRows, row => row.Label == "子どもの生活費");

        Assert.Equal(labels.IndexOf("基本生活費") + 1, labels.IndexOf("子どもの生活費"));
        Assert.Equal("expense", childLivingCostRow.CategoryKey);
        Assert.Equal("61.4", childLivingCostRow.Values[0]);
    }

    [Fact]
    public void ToResultViewModel_AddsHousingMaintenanceRowAfterHousingLoanRepayment()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(
                2026,
                expenses: new AnnualExpense(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 500_000)));

        var viewModel = LifePlanPageMapper.ToResultViewModel(result);
        var labels = viewModel.CashFlowRows.Select(row => row.Label).ToList();
        var housingMaintenanceRow = Assert.Single(viewModel.CashFlowRows, row => row.Label == "住宅維持費");

        Assert.Equal(labels.IndexOf("住宅ローン返済") + 1, labels.IndexOf("住宅維持費"));
        Assert.Equal("50.0", housingMaintenanceRow.Values[0]);
    }

    [Fact]
    public void ToResultViewModel_CreatesChartPointsInManYen()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(
                2026,
                husbandIncome: new PersonAnnualIncome(1_200_000, 300_000, 0),
                wifeIncome: new PersonAnnualIncome(800_000, 0, 200_000),
                expenses: new AnnualExpense(600_000, 120_000, 80_000, 40_000, 30_000, 20_000, 10_000, 50_000, 70_000, 0, 0),
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

    [Fact]
    public void ToResultViewModel_CreatesFirstYearSummary()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(
                2026,
                husbandIncome: new PersonAnnualIncome(5_000_000, 0, 0),
                expenses: new AnnualExpense(2_400_000, 0, 0, 0, 0, 0, 0, 0, 0, 613_836, 0)),
            CreateAnnualRow(2027));

        var summary = LifePlanPageMapper.ToResultViewModel(result).FirstYearSummary;

        Assert.Equal("500.0", summary.TotalIncomeText);
        Assert.Equal("301.4", summary.TotalExpenseText);
        Assert.Equal("198.6", summary.AnnualBalanceText);
    }

    [Fact]
    public void ToResultViewModel_ShowsFirstYearAmountForAutoCostStartingThisYear()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(2026, expenses: CreateAutoCostExpense(childLivingCostYen: 613_836)));

        var autoCost = Assert.Single(
            LifePlanPageMapper.ToResultViewModel(result).AutoCosts,
            cost => cost.Label == "子どもの生活費");

        Assert.Equal("初年度 61.4万円", autoCost.AmountText);
    }

    [Fact]
    public void ToResultViewModel_ShowsStartYearAndAmountForFutureAutoCost()
    {
        var result = CreateCalculationResult(
            HousingMaintenanceStatus.Calculated,
            CreateAnnualRow(2026),
            CreateAnnualRow(2027),
            CreateAnnualRow(2028, expenses: CreateAutoCostExpense(housingMaintenanceYen: 520_200)));

        var autoCost = Assert.Single(
            LifePlanPageMapper.ToResultViewModel(result).AutoCosts,
            cost => cost.Label == "住宅維持費");

        Assert.Equal("2028年から 52.0万円", autoCost.AmountText);
        Assert.Null(autoCost.Note);
    }

    [Fact]
    public void ToResultViewModel_ShowsStartYearAndAmountForFutureChildLivingCost()
    {
        var result = CreateCalculationResult(
            CreateAnnualRow(2026),
            CreateAnnualRow(2027),
            CreateAnnualRow(2028, expenses: CreateAutoCostExpense(childLivingCostYen: 638_627)));

        var autoCost = Assert.Single(
            LifePlanPageMapper.ToResultViewModel(result).AutoCosts,
            cost => cost.Label == "子どもの生活費");

        Assert.Equal("2028年から 63.9万円", autoCost.AmountText);
    }

    [Fact]
    public void ToResultViewModel_ShowsNoAutoCostWhenNeverAdded()
    {
        var result = CreateCalculationResult(CreateAnnualRow(2026), CreateAnnualRow(2027));

        var autoCosts = LifePlanPageMapper.ToResultViewModel(result).AutoCosts;

        Assert.Equal(["子どもの生活費", "住宅維持費"], autoCosts.Select(cost => cost.Label));
        Assert.All(autoCosts, cost => Assert.Equal("計上なし", cost.AmountText));
    }

    [Fact]
    public void ToResultViewModel_ShowsNoteWhenHousingPriceIsMissing()
    {
        var result = CreateCalculationResult(HousingMaintenanceStatus.PriceMissing, CreateAnnualRow(2026));

        var autoCost = Assert.Single(
            LifePlanPageMapper.ToResultViewModel(result).AutoCosts,
            cost => cost.Label == "住宅維持費");

        Assert.Equal("計上なし", autoCost.AmountText);
        Assert.Equal("住宅価格が未設定のため維持費を含めていません", autoCost.Note);
    }

    private static AnnualExpense CreateAutoCostExpense(long childLivingCostYen = 0, long housingMaintenanceYen = 0)
    {
        return new AnnualExpense(0, 0, 0, 0, 0, 0, 0, 0, 0, childLivingCostYen, housingMaintenanceYen);
    }

    private static LifePlanCalculationResult CreateCalculationResult(params AnnualCashFlowRow[] rows)
    {
        return CreateCalculationResult(HousingMaintenanceStatus.NotPlanned, rows);
    }

    private static LifePlanCalculationResult CreateCalculationResult(
        HousingMaintenanceStatus housingMaintenanceStatus,
        params AnnualCashFlowRow[] rows)
    {
        return new LifePlanCalculationResult(
            rows[0].Year,
            rows[^1].Year,
            rows,
            housingMaintenanceStatus,
            "test");
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
            expenses ?? new AnnualExpense(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            0,
            savingsBalanceWithoutReturnYen,
            savingsBalanceWithReturnYen);
    }
}
