using LifePlan.Application.Dto.MicroCms;
using LifePlan.Application.Mappers;
using LifePlan.Application.ReferenceData;
using LifePlan.ViewModels.Articles;

namespace LifePlan.Application.Factories
{
    public static class ArticlePageViewModelFactory
    {
        private const string ListUnavailableMessage = "記事を読み込めませんでした。時間をおいて再度お試しください。";

        public static ArticleListViewModel CreateListPage(
            MicroCmsArticleListResponseDto articles,
            MicroCmsArticleListResponseDto recentArticles,
            IReadOnlyDictionary<string, int> categoryCounts,
            ArticleCategoryEntry? selectedCategory,
            int currentPage,
            int pageSize,
            int recentArticleCount)
        {
            var selectedSlug = selectedCategory?.Slug;

            return new ArticleListViewModel
            {
                Articles = articles.Contents.Select(ArticlePageMapper.ToListItemViewModel).ToList(),
                CategoryFilters = CreateCategoryFilters(selectedSlug),
                Sidebar = CreateSidebar(recentArticles, categoryCounts, selectedSlug, recentArticleCount),
                Pagination = CreatePagination(articles.TotalCount, currentPage, pageSize, selectedSlug),
                SelectedCategorySlug = selectedSlug
            };
        }

        public static ArticleListViewModel CreateUnavailableListPage(
            ArticleCategoryEntry? selectedCategory,
            int currentPage,
            int pageSize,
            int recentArticleCount)
        {
            var selectedSlug = selectedCategory?.Slug;

            return new ArticleListViewModel
            {
                CategoryFilters = CreateCategoryFilters(selectedSlug),
                Sidebar = CreateSidebar(
                    new MicroCmsArticleListResponseDto(),
                    new Dictionary<string, int>(),
                    selectedSlug,
                    recentArticleCount),
                Pagination = CreatePagination(totalCount: 0, currentPage, pageSize, selectedSlug),
                SelectedCategorySlug = selectedSlug,
                HasError = true,
                ErrorMessage = ListUnavailableMessage
            };
        }

        public static ArticleDetailViewModel CreateDetailPage(
            MicroCmsArticleDto article,
            string sanitizedBodyHtml,
            MicroCmsArticleListResponseDto recentArticles,
            IReadOnlyDictionary<string, int> categoryCounts,
            int recentArticleCount)
        {
            var primaryCategoryDisplayName = ArticlePageMapper.GetPrimaryCategoryDisplayName(article);
            var category = ArticleCategoryCatalog.FindByDisplayName(primaryCategoryDisplayName);
            var categoryUrl = category is null ? null : ArticleUrlFactory.CreateListUrl(category.Slug);
            var breadcrumbs = CreateDetailBreadcrumbs(article, category);
            var sidebar = CreateSidebar(
                recentArticles,
                categoryCounts,
                selectedSlug: null,
                recentArticleCount: recentArticleCount,
                excludedArticleSlug: article.Slug);

            return ArticlePageMapper.ToDetailViewModel(
                article,
                sanitizedBodyHtml,
                sidebar,
                categoryUrl,
                breadcrumbs);
        }

        public static ArticleDetailViewModel CreateUnavailableDetailPage(string title, string message)
        {
            return new ArticleDetailViewModel
            {
                Title = title,
                Description = string.Empty,
                MetaDescription = message,
                PointSummary = string.Empty,
                CategoryDisplayName = string.Empty,
                PublishedDateText = string.Empty,
                ThumbnailUrl = string.Empty,
                BodyText = string.Empty,
                HasError = true,
                ErrorMessage = message
            };
        }

        private static IReadOnlyList<ArticleBreadcrumbItemViewModel> CreateDetailBreadcrumbs(
            MicroCmsArticleDto article,
            ArticleCategoryEntry? category)
        {
            var title = string.IsNullOrWhiteSpace(article.Title)
                ? "無題の記事"
                : article.Title.Trim();
            var breadcrumbs = new List<ArticleBreadcrumbItemViewModel>
            {
                new()
                {
                    Text = "記事一覧",
                    Url = ArticleUrlFactory.CreateListUrl()
                }
            };

            if (category is not null)
            {
                breadcrumbs.Add(new ArticleBreadcrumbItemViewModel
                {
                    Text = category.DisplayName,
                    Url = ArticleUrlFactory.CreateListUrl(category.Slug)
                });
            }

            breadcrumbs.Add(new ArticleBreadcrumbItemViewModel
            {
                Text = title,
                IsCurrent = true
            });

            return breadcrumbs;
        }

        private static ArticleSidebarViewModel CreateSidebar(
            MicroCmsArticleListResponseDto recentArticles,
            IReadOnlyDictionary<string, int> categoryCounts,
            string? selectedSlug,
            int recentArticleCount,
            string? excludedArticleSlug = null)
        {
            return new ArticleSidebarViewModel
            {
                CtaHeading = "ふたりの将来のお金を見える化しませんか？",
                CtaButtonText = "無料でシミュレーション",
                CtaUrl = "/LifePlan",
                RecentArticles = recentArticles.Contents
                    .Where(article => !IsSameSlug(article.Slug, excludedArticleSlug))
                    .Take(recentArticleCount)
                    .Select(ArticlePageMapper.ToListItemViewModel)
                    .ToList(),
                Categories = ArticleCategoryCatalog.All
                    .Select(category => new ArticleCategoryViewModel
                    {
                        DisplayName = category.DisplayName,
                        Slug = category.Slug,
                        Url = ArticleUrlFactory.CreateListUrl(category.Slug),
                        IsSelected = string.Equals(category.Slug, selectedSlug, StringComparison.OrdinalIgnoreCase),
                        Count = categoryCounts.TryGetValue(category.Slug, out var count) ? count : 0
                    })
                    .ToList()
            };
        }

        private static IReadOnlyList<ArticleCategoryViewModel> CreateCategoryFilters(string? selectedSlug)
        {
            var categories = new List<ArticleCategoryViewModel>
            {
                new()
                {
                    DisplayName = "すべて",
                    Url = ArticleUrlFactory.CreateListUrl(),
                    IsSelected = string.IsNullOrWhiteSpace(selectedSlug)
                }
            };

            categories.AddRange(ArticleCategoryCatalog.All.Select(category => new ArticleCategoryViewModel
            {
                DisplayName = category.DisplayName,
                Slug = category.Slug,
                Url = ArticleUrlFactory.CreateListUrl(category.Slug),
                IsSelected = string.Equals(category.Slug, selectedSlug, StringComparison.OrdinalIgnoreCase)
            }));

            return categories;
        }

        private static ArticlePaginationViewModel CreatePagination(
            int totalCount,
            int currentPage,
            int pageSize,
            string? selectedSlug)
        {
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            var pageLinks = Enumerable.Range(1, totalPages)
                .Select(page => new ArticlePaginationLinkViewModel
                {
                    Page = page,
                    Url = ArticleUrlFactory.CreateListUrl(selectedSlug, page),
                    IsCurrent = page == currentPage
                })
                .ToList();

            return new ArticlePaginationViewModel
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageLinks = pageLinks,
                PreviousUrl = currentPage > 1 ? ArticleUrlFactory.CreateListUrl(selectedSlug, currentPage - 1) : null,
                NextUrl = currentPage < totalPages ? ArticleUrlFactory.CreateListUrl(selectedSlug, currentPage + 1) : null
            };
        }

        private static bool IsSameSlug(string? currentSlug, string? excludedSlug)
        {
            return !string.IsNullOrWhiteSpace(currentSlug)
                && !string.IsNullOrWhiteSpace(excludedSlug)
                && string.Equals(currentSlug.Trim(), excludedSlug.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
