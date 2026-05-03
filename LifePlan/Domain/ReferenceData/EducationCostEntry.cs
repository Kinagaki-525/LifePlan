namespace LifePlan.Domain.ReferenceData
{
    public record EducationCostEntry(
        string Value,
        string Stage,
        string Type,
        int StartAge,
        int EndAge,
        decimal FirstYearCostManYen,
        decimal LaterYearCostManYen);
}
