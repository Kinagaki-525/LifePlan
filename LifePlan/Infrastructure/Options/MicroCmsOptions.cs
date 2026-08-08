namespace LifePlan.Infrastructure.Options
{
    public class MicroCmsOptions
    {
        public const string SectionName = "MicroCms";

        public string? ServiceDomain { get; set; }

        public string? ApiKey { get; set; }

        public string ArticlesEndpoint { get; set; } = "articles";
    }
}
