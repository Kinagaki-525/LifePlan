namespace LifePlan.Application.ReferenceData
{
    public static class ArticleCategoryCatalog
    {
        public static readonly IReadOnlyList<ArticleCategoryEntry> All =
        [
            new("結婚・結婚式", "marriage"),
            new("家計管理", "household-budget"),
            new("家事", "housework"),
            new("子育て", "parenting"),
            new("その他", "other")
        ];

        public static bool TryGetBySlug(string? slug, out ArticleCategoryEntry category)
        {
            category = All.FirstOrDefault(item => string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase))!;

            return category is not null;
        }

        public static ArticleCategoryEntry? FindByDisplayName(string? displayName)
        {
            return All.FirstOrDefault(item => string.Equals(item.DisplayName, displayName, StringComparison.Ordinal));
        }
    }
}
