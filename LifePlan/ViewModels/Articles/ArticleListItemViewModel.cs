namespace LifePlan.ViewModels.Articles
{
    public class ArticleListItemViewModel
    {
        public required string Title { get; init; }

        public required string Description { get; init; }

        public required string DetailUrl { get; init; }

        public required string CategoryDisplayName { get; init; }

        public required string PublishedDateText { get; init; }

        public required string ThumbnailUrl { get; init; }

        public int? ThumbnailWidth { get; init; }

        public int? ThumbnailHeight { get; init; }
    }
}
