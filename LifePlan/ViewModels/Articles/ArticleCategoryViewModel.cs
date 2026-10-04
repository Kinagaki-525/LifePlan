namespace LifePlan.ViewModels.Articles
{
    public class ArticleCategoryViewModel
    {
        public required string DisplayName { get; init; }

        public string? Slug { get; init; }

        public required string Url { get; init; }

        public bool IsSelected { get; init; }

        public int? Count { get; init; }
    }
}
