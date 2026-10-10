using System.Globalization;
using LifePlan.Domain.Logic;
using LifePlan.Domain.ReferenceData;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Factories
{
    public static class LifePlanExpenseGuidanceFactory
    {
        private const decimal MonthlyReferenceUnitYen = 100m;

        public static LifePlanExpenseGuidanceViewModel Create(LifePlanViewModel page, int startYear, bool isSubmittedInput)
        {
            return Create(page, startYear, isSubmittedInput, SimulationAssumptions.Current);
        }

        public static LifePlanExpenseGuidanceViewModel Create(
            LifePlanViewModel page,
            int startYear,
            bool isSubmittedInput,
            SimulationAssumptions assumptions)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(assumptions);

            var inflationRate = FormatRate(assumptions.InflationRatePercent);
            var housingMaintenanceRate = FormatRate(assumptions.HousingMaintenanceRatePercent);

            return new LifePlanExpenseGuidanceViewModel
            {
                BasicLivingCostNote = "※お子さまの食費・衣服・日用品は自動計算します。その分を除いた月額を入力してください。家賃・教育費は別に計算します。",
                ChildNote = "※年齢に応じた生活費を自動で加算します。教育費は選んだ進路から別に計算します。",
                HousingNote = $"※購入費用（頭金＋借入額）の年{housingMaintenanceRate}%を、税金・保険・修繕などの維持費として自動で加算します。",
                OtherCostNote = "※自由費など、ほかの欄に入力していない支出を入力してください。自動計算する子どもの生活費・住宅維持費は含めないでください。",
                InflationNote = $"※生活費・教育費などは、物価が年{inflationRate}%上がる仮定で計算します（予測値ではありません）。家賃・住宅ローンなどは対象外です。",
                ChildMonthlyReference = isSubmittedInput ? CreateChildMonthlyReference(page, startYear, assumptions) : null,
                ChildMonthlyReferenceByAgeBand = CreateAgeBandReferences(startYear, assumptions),
                ChildMonthlyReferenceCaveat = "※子どもの生活費の概算です。ご家庭の実際の内訳とは一致しません。"
            };
        }

        private static string? CreateChildMonthlyReference(
            LifePlanViewModel page,
            int startYear,
            SimulationAssumptions assumptions)
        {
            var annualTotalYen = 0L;

            for (var index = 0; index < page.Family.Children.Count; index++)
            {
                var hasGraduateSchool = index < page.LifeEvents.EducationPlans.Count &&
                    !string.IsNullOrWhiteSpace(page.LifeEvents.EducationPlans[index].GraduateSchoolOptionValue);

                annualTotalYen += ChildLivingCostCalculator.CalculateAnnualCost(
                    page.Family.Children[index].Age,
                    hasGraduateSchool,
                    startYear,
                    assumptions);
            }

            return annualTotalYen > 0 ? FormatMonthlyReference(annualTotalYen) : null;
        }

        private static IReadOnlyList<string> CreateAgeBandReferences(int startYear, SimulationAssumptions assumptions)
        {
            return assumptions.ChildLivingCosts
                .Select(entry =>
                {
                    var annualCostYen = ChildLivingCostCalculator.CalculateAnnualCost(
                        entry.StartAge,
                        hasGraduateSchool: true,
                        startYear,
                        assumptions);

                    return $"{entry.StartAge}〜{entry.EndAge}歳：{FormatMonthlyReference(annualCostYen)}";
                })
                .ToList();
        }

        private static string FormatMonthlyReference(long annualCostYen)
        {
            var monthlyYen = decimal.Round(
                annualCostYen / 12m / MonthlyReferenceUnitYen,
                0,
                MidpointRounding.AwayFromZero) * MonthlyReferenceUnitYen;

            return $"約{monthlyYen.ToString("#,0", CultureInfo.InvariantCulture)}円";
        }

        private static string FormatRate(decimal ratePercent)
        {
            return ratePercent.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}
