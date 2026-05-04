using LifePlan.Domain.ReferenceData;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Mappers
{
    public static class LifePlanAssumptionMapper
    {
        public static LifePlanAssumptionsViewModel CreateAssumptions()
        {
            return new LifePlanAssumptionsViewModel
            {
                GeneralNotes =
                [
                    "給与・退職金・年金：手取りとして計算",
                    "家賃：値上げを考慮しない",
                    "自動車ローン：組まない"
                ],
                EducationCosts = CreateEducationCostAssumptions()
            };
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
            return manYen.ToString("0.#");
        }
    }
}
