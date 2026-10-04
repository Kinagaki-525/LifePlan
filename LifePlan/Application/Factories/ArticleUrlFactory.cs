namespace LifePlan.Application.Factories
{
    public static class ArticleUrlFactory
    {
        private const string ArticlesPath = "/Articles";

        public static string CreateListUrl(string? categorySlug = null, int? page = null)
        {
            var parameters = new List<string>();

            if (!string.IsNullOrWhiteSpace(categorySlug))
            {
                parameters.Add($"category={Uri.EscapeDataString(categorySlug)}");
            }

            if (page.HasValue && page.Value > 1)
            {
                parameters.Add($"page={page.Value}");
            }

            return parameters.Count == 0
                ? ArticlesPath
                : $"{ArticlesPath}?{string.Join("&", parameters)}";
        }

        public static string CreateDetailUrl(string? slug)
        {
            return string.IsNullOrWhiteSpace(slug)
                ? ArticlesPath
                : $"{ArticlesPath}/{Uri.EscapeDataString(slug.Trim())}";
        }
    }
}
