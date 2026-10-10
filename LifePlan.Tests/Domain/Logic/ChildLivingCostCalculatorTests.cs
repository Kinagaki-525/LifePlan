using LifePlan.Domain.Logic;
using LifePlan.Domain.ReferenceData;

namespace LifePlan.Tests.Domain.Logic;

public class ChildLivingCostCalculatorTests
{
    [Theory]
    [InlineData(0, 613_836)]
    [InlineData(6, 697_068)]
    [InlineData(12, 790_704)]
    [InlineData(15, 842_724)]
    [InlineData(18, 842_724)]
    public void CalculateAnnualCost_AppliesInflationFromPriceBaseYear(int childAge, long expectedYen)
    {
        Assert.Equal(expectedYen, Calculate(childAge, hasGraduateSchool: false, year: 2026));
    }

    [Fact]
    public void CalculateAnnualCost_DoesNotApplyStartYearFactorTwice()
    {
        Assert.Equal(613_836, Calculate(0, hasGraduateSchool: false, year: 2026));
        Assert.Equal(626_113, Calculate(1, hasGraduateSchool: false, year: 2027));
    }

    [Theory]
    [InlineData(2, 613_836)]
    [InlineData(3, 613_836)]
    [InlineData(5, 613_836)]
    [InlineData(6, 697_068)]
    [InlineData(11, 697_068)]
    [InlineData(12, 790_704)]
    [InlineData(14, 790_704)]
    [InlineData(15, 842_724)]
    [InlineData(17, 842_724)]
    [InlineData(18, 842_724)]
    [InlineData(21, 842_724)]
    public void CalculateAnnualCost_SwitchesAtAgeBandBoundaries(int childAge, long expectedYen)
    {
        Assert.Equal(expectedYen, Calculate(childAge, hasGraduateSchool: false, year: 2026));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(22)]
    [InlineData(30)]
    public void CalculateAnnualCost_ReturnsZeroOutsideSupportPeriod(int? childAge)
    {
        Assert.Equal(0, Calculate(childAge, hasGraduateSchool: false, year: 2026));
    }

    [Fact]
    public void CalculateAnnualCost_ExtendsSupportPeriodForGraduateSchool()
    {
        Assert.Equal(842_724, Calculate(22, hasGraduateSchool: true, year: 2026));
        Assert.Equal(842_724, Calculate(23, hasGraduateSchool: true, year: 2026));
        Assert.Equal(0, Calculate(24, hasGraduateSchool: true, year: 2026));
    }

    private static long Calculate(int? childAge, bool hasGraduateSchool, int year)
    {
        return ChildLivingCostCalculator.CalculateAnnualCost(
            childAge,
            hasGraduateSchool,
            year,
            SimulationAssumptions.Current);
    }
}
