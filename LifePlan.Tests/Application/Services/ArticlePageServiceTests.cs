using System.Text.Json;
using LifePlan.Application.Dto.MicroCms;
using LifePlan.Application.Interfaces;
using LifePlan.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace LifePlan.Tests.Application.Services;

public class ArticlePageServiceTests
{
    [Fact]
    public async Task CreateDetailPage_ReturnsDetailPageWhenArticleExists()
    {
        // Arrange
        var repository = new FakeArticleRepository
        {
            ArticleBySlug = CreateArticle("target", "対象記事", metaDescription: "対象記事のメタ説明"),
            ArticlesResponse = new MicroCmsArticleListResponseDto
            {
                Contents =
                [
                    CreateArticle("target", "対象記事", metaDescription: "対象記事のメタ説明"),
                    CreateArticle("recent-1", "新着記事1"),
                    CreateArticle("recent-2", "新着記事2"),
                    CreateArticle("recent-3", "新着記事3"),
                    CreateArticle("recent-4", "新着記事4")
                ],
                TotalCount = 5
            }
        };
        var service = CreateService(repository);

        // Act
        var result = await service.CreateDetailPage("target", CancellationToken.None);

        // Assert
        Assert.Null(result.StatusCode);
        Assert.False(result.Page.HasError);
        Assert.Equal("対象記事", result.Page.Title);
        Assert.Equal("対象記事のメタ説明", result.Page.MetaDescription);
        Assert.Equal("対象記事の説明", result.Page.PointSummary);
        Assert.Equal("2026.06.01", result.Page.PublishedDateText);
        Assert.Equal("家計管理", result.Page.CategoryDisplayName);
        Assert.Equal("/Articles?category=household-budget", result.Page.CategoryUrl);
        Assert.Equal("sanitized:対象記事の本文", result.Page.BodyText);
        Assert.Equal("/Articles", result.Page.BackUrl);
        Assert.Collection(
            result.Page.Breadcrumbs,
            breadcrumb =>
            {
                Assert.Equal("記事一覧", breadcrumb.Text);
                Assert.Equal("/Articles", breadcrumb.Url);
            },
            breadcrumb =>
            {
                Assert.Equal("家計管理", breadcrumb.Text);
                Assert.Equal("/Articles?category=household-budget", breadcrumb.Url);
            },
            breadcrumb =>
            {
                Assert.Equal("対象記事", breadcrumb.Text);
                Assert.Null(breadcrumb.Url);
                Assert.True(breadcrumb.IsCurrent);
            });
        Assert.NotNull(result.Page.Sidebar);
        Assert.DoesNotContain(result.Page.Sidebar.RecentArticles, article => article.Title == "対象記事");
        Assert.Equal(3, result.Page.Sidebar.RecentArticles.Count);
        Assert.Equal(
            new[] { "新着記事1", "新着記事2", "新着記事3" },
            result.Page.Sidebar.RecentArticles.Select(article => article.Title));
    }

    [Fact]
    public async Task CreateDetailPage_ReturnsNotFoundWhenArticleDoesNotExist()
    {
        // Arrange
        var repository = new FakeArticleRepository();
        var service = CreateService(repository);

        // Act
        var result = await service.CreateDetailPage("missing", CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.True(result.Page.HasError);
        Assert.Equal("記事が見つかりません", result.Page.Title);
    }

    [Fact]
    public async Task CreateDetailPage_ReturnsServiceUnavailableWhenRepositoryFails()
    {
        // Arrange
        var repository = new FakeArticleRepository
        {
            ExceptionToThrow = new InvalidOperationException("microCMS failed")
        };
        var service = CreateService(repository);

        // Act
        var result = await service.CreateDetailPage("target", CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, result.StatusCode);
        Assert.True(result.Page.HasError);
        Assert.Equal("記事を読み込めません", result.Page.Title);
    }

    [Fact]
    public async Task CreateDetailPage_FallsBackToPublishedAtAndDescription()
    {
        // Arrange
        var repository = new FakeArticleRepository
        {
            ArticleBySlug = CreateArticle(
                "target",
                "対象記事",
                includePublishedDate: false,
                metaDescription: null),
            ArticlesResponse = new MicroCmsArticleListResponseDto()
        };
        var service = CreateService(repository);

        // Act
        var result = await service.CreateDetailPage("target", CancellationToken.None);

        // Assert
        Assert.Equal("対象記事の説明", result.Page.MetaDescription);
        Assert.Equal("対象記事の説明", result.Page.PointSummary);
        Assert.Equal("2026.05.26", result.Page.PublishedDateText);
    }

    private static ArticlePageService CreateService(IArticleRepository repository)
    {
        return new ArticlePageService(
            repository,
            new FakeArticleHtmlSanitizer(),
            NullLogger<ArticlePageService>.Instance);
    }

    private static MicroCmsArticleDto CreateArticle(
        string slug,
        string title,
        DateTimeOffset? publishedDate = null,
        string? metaDescription = "",
        bool includePublishedDate = true)
    {
        return new MicroCmsArticleDto
        {
            Slug = slug,
            Title = title,
            Description = $"{title}の説明",
            MetaDescription = metaDescription ?? string.Empty,
            Body = $"{title}の本文",
            PublishedAt = new DateTimeOffset(2026, 5, 26, 0, 0, 0, TimeSpan.Zero),
            PublishedDate = includePublishedDate
                ? publishedDate ?? new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero)
                : null,
            Category = JsonDocument.Parse("\"家計管理\"").RootElement.Clone(),
            Thumbnail = new MicroCmsImageDto
            {
                Url = $"https://example.com/{slug}.jpg",
                Width = 640,
                Height = 360
            }
        };
    }

    private sealed class FakeArticleRepository : IArticleRepository
    {
        public MicroCmsArticleDto? ArticleBySlug { get; init; }

        public MicroCmsArticleListResponseDto ArticlesResponse { get; init; } = new();

        public Exception? ExceptionToThrow { get; init; }

        public Task<MicroCmsArticleListResponseDto> GetArticlesAsync(
            int limit,
            int offset,
            string? categoryName,
            CancellationToken cancellationToken)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(ArticlesResponse);
        }

        public Task<MicroCmsArticleDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(ArticleBySlug);
        }
    }

    private sealed class FakeArticleHtmlSanitizer : IArticleHtmlSanitizer
    {
        public string Sanitize(string? html)
        {
            return $"sanitized:{html}";
        }
    }
}
