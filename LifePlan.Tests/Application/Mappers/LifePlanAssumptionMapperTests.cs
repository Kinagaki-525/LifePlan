using LifePlan.Application.Mappers;
using LifePlan.Domain.ReferenceData;

namespace LifePlan.Tests.Application.Mappers;

public class LifePlanAssumptionMapperTests
{
    [Fact]
    public void CreateAssumptions_CreatesGeneralNotes()
    {
        var assumptions = LifePlanAssumptionMapper.CreateAssumptions();

        Assert.Contains("給与・退職金・年金：手取りとして計算", assumptions.GeneralNotes);
        Assert.Contains("家賃：値上げを考慮しない", assumptions.GeneralNotes);
        Assert.Contains("自動車ローン：組まない", assumptions.GeneralNotes);
    }

    [Fact]
    public void CreateAssumptions_CreatesEducationCostsFromReferenceData()
    {
        var assumptions = LifePlanAssumptionMapper.CreateAssumptions();

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

        var notes = LifePlanAssumptionMapper.CreateAssumptions().AutoCostNotes;

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
}
