namespace LifePlan.ViewModels.Articles
{
    public class ArticleDetailViewModel
    {
        public required string Title { get; init; }

        public required string Description { get; init; }

        public required string MetaDescription { get; init; }

        public required string PointSummary { get; init; }

        public required string CategoryDisplayName { get; init; }

        public string? CategoryUrl { get; init; }

        public required string PublishedDateText { get; init; }

        public IReadOnlyList<ArticleBreadcrumbItemViewModel> Breadcrumbs { get; init; } = [];

        public IReadOnlyList<string> Tags { get; init; } = [];

        public required string ThumbnailUrl { get; init; }

        public int? ThumbnailWidth { get; init; }

        public int? ThumbnailHeight { get; init; }

        public required string BodyText { get; init; }

        public string BackUrl { get; init; } = "/Articles";

        public ArticleSidebarViewModel? Sidebar { get; init; }

        public bool HasError { get; init; }

        public string? ErrorMessage { get; init; }
    }
}
