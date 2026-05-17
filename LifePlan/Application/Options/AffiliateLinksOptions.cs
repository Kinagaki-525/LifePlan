namespace LifePlan.Application.Options
{
    public class AffiliateLinksOptions
    {
        public const string SectionName = "AffiliateLinks";

        public AffiliateLinkOptions FpConsultation { get; set; } = new();
    }
}
