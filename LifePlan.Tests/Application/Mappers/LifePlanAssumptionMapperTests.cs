using System.Globalization;
using LifePlan.Application.Mappers;
using LifePlan.Domain.ReferenceData;

namespace LifePlan.Tests.Application.Mappers;

public class LifePlanAssumptionMapperTests
{
    [Fact]
    public void CreateAssumptions_CreatesGeneralNotes()
    {
        var assumptions = LifePlanAssumptionMapper.CreateAssumptions(SimulationAssumptions.Current.Version);

        Assert.Contains("給与・退職金・年金：手取りとして計算", assumptions.GeneralNotes);
        Assert.Contains("家賃：値上げを考慮しない", assumptions.GeneralNotes);
        Assert.Contains("自動車ローン：組まない", assumptions.GeneralNotes);
    }

    [Fact]
    public void CreateAssumptions_CreatesEducationCostsFromReferenceData()
    {
        var assumptions = LifePlanAssumptionMapper.CreateAssumptions(SimulationAssumptions.Current.Version);

        var nursery = Assert.Single(assumptions.EducationCosts, cost => cost.Stage == "保育園（0〜2歳）");
        Assert.Equal(["公立45万円/年、私立55万円/年"], nursery.CostLines);

        var university = Assert.Single(assumptions.EducationCosts, cost => cost.Stage == "大学");
        Assert.Contains("国公立 初年度85万円/年、次年度以降55万円/年", university.CostLines);
        Assert.Contains("私立文系 初年度120万円/年、次年度以降100万円/年", university.CostLines);
        Assert.Contains("私立理系 初年度155万円/年、次年度以降130万円/年", university.CostLines);
    }

    [Fact]
    public void CreateAssumptions_CreatesAutoCostNotesFromSimulationAssumptions()
    {
        var current = SimulationAssumptions.Current;

        var notes = LifePlanAssumptionMapper.CreateAssumptions(SimulationAssumptions.Current.Version).AutoCostNotes;

        Assert.Contains(notes, note => note.Contains("衣類・食費・生活用品") &&
            note.Contains($"{current.ChildSupportEndAge}歳") &&
            note.Contains($"{current.ChildSupportEndAgeWithGraduateSchool}歳"));
        Assert.Contains(notes, note => note.Contains("18歳以降は高校生の額を継続する仮定"));
        Assert.Contains(notes, note => note.Contains("住宅維持費") && note.Contains("年1%"));
        Assert.Contains(notes, note => note.Contains("年2%") && note.Contains("家賃・住宅ローン"));
        Assert.Contains(notes, note => note.Contains("携帯料金・小遣い・医療費") && note.Contains("下宿費"));
        Assert.Contains("生活費と教育費の一部費目には重複が残る概算です", notes);
        Assert.Contains(notes, note => note.Contains(current.Version));
    }

    [Fact]
    public void CreateAssumptions_ShowsGivenAssumptionsVersion()
    {
        var notes = LifePlanAssumptionMapper.CreateAssumptions("2025.04").AutoCostNotes;

        Assert.Contains("前提バージョン：2025.04", notes);
        Assert.DoesNotContain($"前提バージョン：{SimulationAssumptions.Current.Version}", notes);
    }

    [Fact]
    public void CreateAssumptions_FormatsRatesIndependentOfCurrentCulture()
    {
        var assumptions = SimulationAssumptions.Current with
        {
            InflationRatePercent = 1.5m,
            HousingMaintenanceRatePercent = 0.5m
        };
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            var notes = LifePlanAssumptionMapper.CreateAssumptions(assumptions.Version, assumptions).AutoCostNotes;

            Assert.Contains(notes, note => note.StartsWith("物価上昇：") && note.Contains("年1.5%"));
            Assert.Contains(notes, note => note.StartsWith("住宅維持費：") && note.Contains("年0.5%"));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
