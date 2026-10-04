using LifePlan.Application.Factories;
using LifePlan.Application.Interfaces;
using LifePlan.Application.ReferenceData;
using LifePlan.Application.Results;
using LifePlan.ViewModels.Articles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LifePlan.Application.Services
{
    public class ArticlePageService : IArticlePageService
    {
        private const int PageSize = 6;
        private const int RecentArticleCount = 3;
        private const string DetailUnavailableTitle = "記事を読み込めません";
        private const string DetailUnavailableMessage = "記事を読み込めませんでした。時間をおいて再度お試しください。";
        private const string DetailNotFoundTitle = "記事が見つかりません";
        private const string DetailNotFoundMessage = "指定された記事は見つかりませんでした。";

        private readonly IArticleRepository articleRepository;
        private readonly IArticleHtmlSanitizer articleHtmlSanitizer;
        private readonly ILogger<ArticlePageService> logger;

        public ArticlePageService(
            IArticleRepository articleRepository,
            IArticleHtmlSanitizer articleHtmlSanitizer,
            ILogger<ArticlePageService> logger)
        {
            this.articleRepository = articleRepository;
            this.articleHtmlSanitizer = articleHtmlSanitizer;
            this.logger = logger;
        }

        public async Task<ArticleListResult> CreateListPage(
            string? category,
            int? page,
            CancellationToken cancellationToken)
        {
            var currentPage = NormalizePage(page);
            var selectedCategory = ResolveSelectedCategory(category);

            if (IsInvalidCategory(category, selectedCategory))
            {
                return new ArticleListResult
                {
                    Page = CreateUnavailableListPage(null, currentPage),
                    RedirectUrl = ArticleUrlFactory.CreateListUrl()
                };
            }

            try
            {
                var articles = await articleRepository.GetArticlesAsync(
                    PageSize,
                    (currentPage - 1) * PageSize,
                    selectedCategory?.DisplayName,
                    cancellationToken);

                var totalPages = Math.Max(1, (int)Math.Ceiling(articles.TotalCount / (double)PageSize));

                if (currentPage > totalPages)
                {
                    return new ArticleListResult
                    {
                        Page = CreateUnavailableListPage(selectedCategory, currentPage),
                        RedirectUrl = ArticleUrlFactory.CreateListUrl(selectedCategory?.Slug)
                    };
                }

                var recentArticles = await articleRepository.GetArticlesAsync(
                    RecentArticleCount,
                    offset: 0,
                    categoryName: null,
                    cancellationToken);

                var categoryCounts = await CreateCategoryCounts(cancellationToken);
                var pageViewModel = ArticlePageViewModelFactory.CreateListPage(
                    articles,
                    recentArticles,
                    categoryCounts,
                    selectedCategory,
                    currentPage,
                    PageSize,
                    RecentArticleCount);

                return new ArticleListResult
                {
                    Page = pageViewModel
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to create article list page.");

                return new ArticleListResult
                {
                    Page = CreateUnavailableListPage(selectedCategory, currentPage),
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
            }
        }

        public async Task<ArticleDetailResult> CreateDetailPage(string? slug, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return CreateNotFoundDetailResult();
            }

            try
            {
                var article = await articleRepository.GetArticleBySlugAsync(slug.Trim(), cancellationToken);

                if (article is null)
                {
                    return CreateNotFoundDetailResult();
                }

                var recentArticles = await articleRepository.GetArticlesAsync(
                    limit: RecentArticleCount + 1,
                    offset: 0,
                    categoryName: null,
                    cancellationToken);

                var categoryCounts = await CreateCategoryCounts(cancellationToken);
                var sanitizedBodyHtml = articleHtmlSanitizer.Sanitize(article.Body);
                var pageViewModel = ArticlePageViewModelFactory.CreateDetailPage(
                    article,
                    sanitizedBodyHtml,
                    recentArticles,
                    categoryCounts,
                    RecentArticleCount);

                return new ArticleDetailResult
                {
                    Page = pageViewModel
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to create article detail page.");

                return new ArticleDetailResult
                {
                    Page = ArticlePageViewModelFactory.CreateUnavailableDetailPage(
                        DetailUnavailableTitle,
                        DetailUnavailableMessage),
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
            }
        }

        private async Task<IReadOnlyDictionary<string, int>> CreateCategoryCounts(CancellationToken cancellationToken)
        {
            var counts = new Dictionary<string, int>();

            foreach (var category in ArticleCategoryCatalog.All)
            {
                var response = await articleRepository.GetArticlesAsync(
                    limit: 1,
                    offset: 0,
                    category.DisplayName,
                    cancellationToken);

                counts[category.Slug] = response.TotalCount;
            }

            return counts;
        }

        private static int NormalizePage(int? page)
        {
            return page.GetValueOrDefault(1) < 1
                ? 1
                : page.GetValueOrDefault(1);
        }

        private static ArticleCategoryEntry? ResolveSelectedCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return null;
            }

            return ArticleCategoryCatalog.TryGetBySlug(category.Trim(), out var selectedCategory)
                ? selectedCategory
                : null;
        }

        private static bool IsInvalidCategory(string? category, ArticleCategoryEntry? selectedCategory)
        {
            return !string.IsNullOrWhiteSpace(category) && selectedCategory is null;
        }

        private static ArticleListViewModel CreateUnavailableListPage(
            ArticleCategoryEntry? selectedCategory,
            int currentPage)
        {
            return ArticlePageViewModelFactory.CreateUnavailableListPage(
                selectedCategory,
                currentPage,
                PageSize,
                RecentArticleCount);
        }

        private static ArticleDetailResult CreateNotFoundDetailResult()
        {
            return new ArticleDetailResult
            {
                Page = ArticlePageViewModelFactory.CreateUnavailableDetailPage(
                    DetailNotFoundTitle,
                    DetailNotFoundMessage),
                StatusCode = StatusCodes.Status404NotFound
            };
        }
    }
}
