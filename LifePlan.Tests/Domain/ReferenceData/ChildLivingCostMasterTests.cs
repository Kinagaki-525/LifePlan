using LifePlan.Domain.ReferenceData;

namespace LifePlan.Tests.Domain.ReferenceData;

public class ChildLivingCostMasterTests
{
    [Fact]
    public void Entries_CoverAgesZeroThroughTwentyThreeWithoutGapsOrOverlaps()
    {
        var entries = ChildLivingCostMaster.Entries;

        Assert.Equal(0, entries[0].StartAge);
        Assert.Equal(23, entries[^1].EndAge);

        for (var index = 1; index < entries.Count; index++)
        {
            Assert.Equal(entries[index - 1].EndAge + 1, entries[index].StartAge);
        }
    }

    [Fact]
    public void Entries_MatchSpecifiedAnnualCosts()
    {
        Assert.Equal(
            [
                (0, 2, 590_000L),
                (3, 5, 590_000L),
                (6, 11, 670_000L),
                (12, 14, 760_000L),
                (15, 17, 810_000L),
                (18, 23, 810_000L)
            ],
            ChildLivingCostMaster.Entries.Select(entry => (entry.StartAge, entry.EndAge, entry.AnnualCostYen)));
    }

    [Fact]
    public void Current_UsesSpecifiedAssumptions()
    {
        var assumptions = SimulationAssumptions.Current;

        Assert.Equal(2m, assumptions.InflationRatePercent);
        Assert.Equal(1m, assumptions.HousingMaintenanceRatePercent);
        Assert.Equal(22, assumptions.ChildSupportEndAge);
        Assert.Equal(24, assumptions.ChildSupportEndAgeWithGraduateSchool);
        Assert.Same(ChildLivingCostMaster.Entries, assumptions.ChildLivingCosts);
        Assert.Equal(2024, assumptions.ChildLivingCostPriceBaseYear);
        Assert.Equal(["2"], assumptions.SupportedCalculationSpecVersions);
        Assert.Equal("2", assumptions.CurrentCalculationSpecVersion);
        Assert.False(string.IsNullOrWhiteSpace(assumptions.Version));
    }
}
