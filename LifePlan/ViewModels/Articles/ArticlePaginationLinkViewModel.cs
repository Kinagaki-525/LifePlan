namespace LifePlan.ViewModels.Articles
{
    public class ArticlePaginationLinkViewModel
    {
        public required int Page { get; init; }

        public required string Url { get; init; }

        public bool IsCurrent { get; init; }
    }
}
