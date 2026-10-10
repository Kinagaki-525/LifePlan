using System.Globalization;
using LifePlan.Domain.ReferenceData;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Mappers
{
    public static class LifePlanAssumptionMapper
    {
        public static LifePlanAssumptionsViewModel CreateAssumptions(string assumptionsVersion)
        {
            return CreateAssumptions(assumptionsVersion, SimulationAssumptions.Current);
        }

        public static LifePlanAssumptionsViewModel CreateAssumptions(
            string assumptionsVersion,
            SimulationAssumptions assumptions)
        {
            ArgumentNullException.ThrowIfNull(assumptions);

            return new LifePlanAssumptionsViewModel
            {
                GeneralNotes =
                [
                    "給与・退職金・年金：手取りとして計算",
                    "家賃：値上げを考慮しない",
                    "自動車ローン：組まない"
                ],
                EducationCosts = CreateEducationCostAssumptions(),
                AutoCostNotes = CreateAutoCostNotes(assumptions, assumptionsVersion)
            };
        }

        private static IReadOnlyList<string> CreateAutoCostNotes(
            SimulationAssumptions assumptions,
            string assumptionsVersion)
        {
            var inflationRate = FormatRate(assumptions.InflationRatePercent);
            var housingMaintenanceRate = FormatRate(assumptions.HousingMaintenanceRatePercent);
            var extendedCostStartAge = assumptions.ChildLivingCosts[^1].StartAge;

            return
            [
                $"子どもの生活費：衣類・食費・生活用品（玩具・書籍・文房具等を含む）の概算。{assumptions.ChildSupportEndAge}歳になる年から計上しない（大学院を選択した子は{assumptions.ChildSupportEndAgeWithGraduateSchool}歳）",
                $"子どもの生活費：{extendedCostStartAge}歳以降は高校生の額を継続する仮定。{assumptions.ChildLivingCostPriceBaseYear}年の調査額を年{inflationRate}%で補正",
                $"住宅維持費：購入費用（頭金＋借入額）の年{housingMaintenanceRate}%を、税金・保険・修繕などの維持費として購入年から計上",
                $"物価上昇：生活費・子どもの生活費・その他支出・教育費・旅行その他・住宅維持費に年{inflationRate}%を適用。家賃・住宅ローン・頭金・自動車・結婚は対象外",
                "自動計上しない費用：子どもの携帯料金・小遣い・医療費、大学の下宿費など（必要に応じてその他支出に入力）",
                "生活費と教育費の一部費目には重複が残る概算です",
                $"前提バージョン：{assumptionsVersion}"
            ];
        }

        private static string FormatRate(decimal ratePercent)
        {
            return ratePercent.ToString("0.#", CultureInfo.InvariantCulture);
        }

        private static IReadOnlyList<EducationCostAssumptionViewModel> CreateEducationCostAssumptions()
        {
            return EducationCostMaster.Entries
                .GroupBy(entry => entry.Stage)
                .Select(group => new EducationCostAssumptionViewModel
                {
                    Stage = ToEducationStageLabel(group),
                    CostLines = ToEducationCostLines(group)
                })
                .ToList();
        }

        private static string ToEducationStageLabel(IGrouping<string, EducationCostEntry> entries)
        {
            var firstEntry = entries.First();

            return entries.Key == "保育園"
                ? $"{entries.Key}（{firstEntry.StartAge}〜{firstEntry.EndAge}歳）"
                : entries.Key;
        }

        private static IReadOnlyList<string> ToEducationCostLines(IGrouping<string, EducationCostEntry> entries)
        {
            if (entries.All(entry => entry.FirstYearCostManYen == entry.LaterYearCostManYen))
            {
                return
                [
                    string.Join("、", entries.Select(entry =>
                        $"{entry.Type}{FormatManYen(entry.FirstYearCostManYen)}万円/年"))
                ];
            }

            return entries
                .Select(entry => $"{entry.Type} 初年度{FormatManYen(entry.FirstYearCostManYen)}万円/年、次年度以降{FormatManYen(entry.LaterYearCostManYen)}万円/年")
                .ToList();
        }

        private static string FormatManYen(decimal manYen)
        {
            return manYen.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}
