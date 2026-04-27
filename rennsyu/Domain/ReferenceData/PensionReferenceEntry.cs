namespace rennsyu.Domain.ReferenceData
{
    public record PensionReferenceEntry(
        string Value,
        string DisplayName,
        long AnnualAmountYen,
        long MonthlyAmountYen);
}
