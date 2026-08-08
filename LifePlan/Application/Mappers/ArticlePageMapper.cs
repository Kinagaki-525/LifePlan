using System.Text.Json;
using LifePlan.Application.Dto.MicroCms;
using LifePlan.Application.Factories;
using LifePlan.ViewModels.Articles;

namespace LifePlan.Application.Mappers
{
    public static class ArticlePageMapper
    {
        private static readonly TimeSpan JapanOffset = TimeSpan.FromHours(9);

        public static ArticleListItemViewModel ToListItemViewModel(MicroCmsArticleDto article)
        {
            var title = TrimOrDefault(article.Title, "無題の記事");
            var categoryDisplayName = GetCategoryDisplayName(article);

            return new ArticleListItemViewModel
            {
                Title = title,
                Description = TrimOrDefault(article.Description, string.Empty),
                DetailUrl = ArticleUrlFactory.CreateDetailUrl(article.Slug),
                CategoryDisplayName = categoryDisplayName,
                PublishedDateText = ToPublishedDateText(article.PublishedDate, article.PublishedAt),
                ThumbnailUrl = article.Thumbnail?.Url ?? string.Empty,
                ThumbnailWidth = article.Thumbnail?.Width,
                ThumbnailHeight = article.Thumbnail?.Height
            };
        }

        public static ArticleDetailViewModel ToDetailViewModel(
            MicroCmsArticleDto article,
            string sanitizedBodyHtml,
            ArticleSidebarViewModel sidebar,
            string? categoryUrl,
            IReadOnlyList<ArticleBreadcrumbItemViewModel> breadcrumbs)
        {
            var title = TrimOrDefault(article.Title, "無題の記事");
            var description = TrimOrDefault(article.Description, string.Empty);
            var metaDescription = TrimOrDefault(article.MetaDescription, description);

            return new ArticleDetailViewModel
            {
                Title = title,
                Description = description,
                MetaDescription = metaDescription,
                PointSummary = description,
                CategoryDisplayName = GetCategoryDisplayName(article),
                CategoryUrl = categoryUrl,
                PublishedDateText = ToPublishedDateText(article.PublishedDate, article.PublishedAt),
                Breadcrumbs = breadcrumbs,
                Tags = SplitTags(article.Tags),
                ThumbnailUrl = article.Thumbnail?.Url ?? string.Empty,
                ThumbnailWidth = article.Thumbnail?.Width,
                ThumbnailHeight = article.Thumbnail?.Height,
                BodyText = sanitizedBodyHtml,
                Sidebar = sidebar
            };
        }

        public static string GetCategoryDisplayName(MicroCmsArticleDto article)
        {
            return GetPrimaryCategoryDisplayName(article) ?? "その他";
        }

        public static string? GetPrimaryCategoryDisplayName(MicroCmsArticleDto article)
        {
            return ExtractCategoryDisplayName(article.Category);
        }

        private static string ToPublishedDateText(DateTimeOffset? publishedDate, DateTimeOffset? publishedAt)
        {
            var displayDate = publishedDate ?? publishedAt;

            return displayDate.HasValue
                ? displayDate.Value.ToOffset(JapanOffset).ToString("yyyy.MM.dd")
                : string.Empty;
        }

        private static IReadOnlyList<string> SplitTags(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return [];
            }

            return tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList();
        }

        private static string? ExtractCategoryDisplayName(JsonElement? category)
        {
            if (!category.HasValue)
            {
                return null;
            }

            return category.Value.ValueKind switch
            {
                JsonValueKind.String => NormalizeCategoryName(category.Value.GetString()),
                JsonValueKind.Array => ExtractFirstCategoryName(category.Value),
                _ => null
            };
        }

        private static string? ExtractFirstCategoryName(JsonElement categoryArray)
        {
            foreach (var item in categoryArray.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var categoryName = NormalizeCategoryName(item.GetString());

                    if (!string.IsNullOrWhiteSpace(categoryName))
                    {
                        return categoryName;
                    }
                }
            }

            return null;
        }

        private static string? NormalizeCategoryName(string? categoryName)
        {
            var trimmed = categoryName?.Trim();

            return string.IsNullOrWhiteSpace(trimmed)
                ? null
                : trimmed;
        }

        private static string TrimOrDefault(string? value, string fallback)
        {
            var trimmed = value?.Trim();

            return string.IsNullOrWhiteSpace(trimmed)
                ? fallback
                : trimmed;
        }
    }
}
