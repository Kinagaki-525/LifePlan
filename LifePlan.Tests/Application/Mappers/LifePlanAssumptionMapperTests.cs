using LifePlan.Application.Mappers;

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
}
