using LifePlan.Application.Results;

namespace LifePlan.Application.Interfaces
{
    public interface IArticlePageService
    {
        Task<ArticleListResult> CreateListPage(string? category, int? page, CancellationToken cancellationToken);

        Task<ArticleDetailResult> CreateDetailPage(string? slug, CancellationToken cancellationToken);
    }
}
