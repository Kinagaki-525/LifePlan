namespace LifePlan.Domain.ReferenceData
{
    public record ChildLivingCostEntry(
        int StartAge,
        int EndAge,
        long AnnualCostYen,
        string Basis);
}
