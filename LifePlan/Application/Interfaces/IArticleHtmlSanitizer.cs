namespace LifePlan.Application.Interfaces
{
    public interface IArticleHtmlSanitizer
    {
        string Sanitize(string? html);
    }
}
