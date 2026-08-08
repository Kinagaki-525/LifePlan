using LifePlan.ViewModels.Articles;

namespace LifePlan.Application.Results
{
    public class ArticleListResult
    {
        public required ArticleListViewModel Page { get; init; }

        public string? RedirectUrl { get; init; }

        public int? StatusCode { get; init; }

        public bool ShouldRedirect => !string.IsNullOrWhiteSpace(RedirectUrl);
    }
}
