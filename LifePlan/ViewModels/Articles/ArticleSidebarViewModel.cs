namespace LifePlan.ViewModels.Articles
{
    public class ArticleSidebarViewModel
    {
        public required string CtaHeading { get; init; }

        public required string CtaButtonText { get; init; }

        public required string CtaUrl { get; init; }

        public IReadOnlyList<ArticleListItemViewModel> RecentArticles { get; init; } = [];

        public IReadOnlyList<ArticleCategoryViewModel> Categories { get; init; } = [];
    }
}
