using LifePlan.Application.Dto.MicroCms;

namespace LifePlan.Application.Interfaces
{
    public interface IArticleRepository
    {
        Task<MicroCmsArticleListResponseDto> GetArticlesAsync(
            int limit,
            int offset,
            string? categoryName,
            CancellationToken cancellationToken);

        Task<MicroCmsArticleDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken);
    }
}
