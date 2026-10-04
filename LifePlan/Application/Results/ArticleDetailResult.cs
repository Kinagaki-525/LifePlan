using LifePlan.ViewModels.Articles;

namespace LifePlan.Application.Results
{
    public class ArticleDetailResult
    {
        public required ArticleDetailViewModel Page { get; init; }

        public int? StatusCode { get; init; }
    }
}
