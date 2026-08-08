namespace LifePlan.ViewModels.Articles
{
    public class ArticleBreadcrumbItemViewModel
    {
        public required string Text { get; init; }

        public string? Url { get; init; }

        public bool IsCurrent { get; init; }
    }
}
