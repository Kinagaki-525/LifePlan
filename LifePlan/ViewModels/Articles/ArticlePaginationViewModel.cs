namespace LifePlan.ViewModels.Articles
{
    public class ArticlePaginationViewModel
    {
        public int CurrentPage { get; init; }

        public int TotalPages { get; init; }

        public IReadOnlyList<ArticlePaginationLinkViewModel> PageLinks { get; init; } = [];

        public string? PreviousUrl { get; init; }

        public string? NextUrl { get; init; }

        public bool ShouldShow => TotalPages > 1;
    }
}
