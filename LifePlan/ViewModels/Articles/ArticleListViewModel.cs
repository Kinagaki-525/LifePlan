namespace LifePlan.ViewModels.Articles
{
    public class ArticleListViewModel
    {
        public IReadOnlyList<ArticleListItemViewModel> Articles { get; init; } = [];

        public IReadOnlyList<ArticleCategoryViewModel> CategoryFilters { get; init; } = [];

        public required ArticleSidebarViewModel Sidebar { get; init; }

        public required ArticlePaginationViewModel Pagination { get; init; }

        public string? SelectedCategorySlug { get; init; }

        public bool HasError { get; init; }

        public string? ErrorMessage { get; init; }

        public bool IsEmpty => !HasError && Articles.Count == 0;
    }
}
