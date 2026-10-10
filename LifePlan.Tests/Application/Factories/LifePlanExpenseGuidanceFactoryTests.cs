using LifePlan.Application.Factories;
using LifePlan.Application.Mappers;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Tests.Application.Factories;

public class LifePlanExpenseGuidanceFactoryTests
{
    private const int StartYear = 2026;

    [Fact]
    public void Create_ShowsAgeBandReferencesBeforeSubmission()
    {
        var page = CreatePage(0);

        var guidance = LifePlanExpenseGuidanceFactory.Create(page, StartYear, isSubmittedInput: false);

        Assert.Null(guidance.ChildMonthlyReference);
        Assert.Equal(6, guidance.ChildMonthlyReferenceByAgeBand.Count);
        Assert.Equal("0〜2歳：約51,200円", guidance.ChildMonthlyReferenceByAgeBand[0]);
        Assert.Equal("18〜23歳：約70,200円", guidance.ChildMonthlyReferenceByAgeBand[^1]);
    }

    [Fact]
    public void Create_ShowsMonthlyTotalForSubmittedChildren()
    {
        var page = CreatePage(0);

        var guidance = LifePlanExpenseGuidanceFactory.Create(page, StartYear, isSubmittedInput: true);

        Assert.Equal("約51,200円", guidance.ChildMonthlyReference);
    }

    [Fact]
    public void Create_DoesNotShowMonthlyTotalWhenSubmittedWithoutCurrentChildren()
    {
        var withoutChildren = LifePlanExpenseGuidanceFactory.Create(CreatePage(), StartYear, isSubmittedInput: true);
        var unbornChild = LifePlanExpenseGuidanceFactory.Create(CreatePage(-1), StartYear, isSubmittedInput: true);

        Assert.Null(withoutChildren.ChildMonthlyReference);
        Assert.Null(unbornChild.ChildMonthlyReference);
    }

    [Fact]
    public void Create_IncludesTwentyTwoYearOldOnlyWhenGraduateSchoolIsSelected()
    {
        var withoutGraduateSchool = CreatePage(22);
        var withGraduateSchool = CreatePage(22);
        withGraduateSchool.LifeEvents.EducationPlans[0].GraduateSchoolOptionValue = "graduate_public";

        var withoutGuidance = LifePlanExpenseGuidanceFactory.Create(withoutGraduateSchool, StartYear, isSubmittedInput: true);
        var withGuidance = LifePlanExpenseGuidanceFactory.Create(withGraduateSchool, StartYear, isSubmittedInput: true);

        Assert.Null(withoutGuidance.ChildMonthlyReference);
        Assert.Equal("約70,200円", withGuidance.ChildMonthlyReference);
    }

    [Fact]
    public void Create_BuildsRateNotesFromSimulationAssumptions()
    {
        var guidance = LifePlanExpenseGuidanceFactory.Create(CreatePage(), StartYear, isSubmittedInput: false);

        Assert.Contains("年1%", guidance.HousingNote);
        Assert.Contains("年2%", guidance.InflationNote);
        Assert.Contains("住宅ローン", guidance.InflationNote);
        Assert.Contains("除いた月額", guidance.BasicLivingCostNote);
        Assert.Contains("含めないでください", guidance.OtherCostNote);
        Assert.Contains("教育費", guidance.ChildNote);
        Assert.Contains("一致しません", guidance.ChildMonthlyReferenceCaveat);
    }

    private static LifePlanViewModel CreatePage(params int[] childAges)
    {
        var page = new LifePlanViewModel
        {
            Family = new FamilyInputViewModel { Children = LifePlanPageMapper.CreateChildInputs() },
            LifeEvents = new LifeEventInputViewModel { EducationPlans = LifePlanPageMapper.CreateEducationPlans() }
        };

        for (var index = 0; index < childAges.Length; index++)
        {
            page.Family.Children[index].Age = childAges[index];
        }

        return page;
    }
}
