using Ganss.Xss;
using LifePlan.Application.Interfaces;
using System.Net;
using System.Text.RegularExpressions;

namespace LifePlan.Application.Services
{
    public class ArticleHtmlSanitizer : IArticleHtmlSanitizer
    {
        private static readonly Regex EscapedAnchorParagraphRegex = new(
            @"<p>\s*(?<anchor>&lt;a\b[\s\S]*?&lt;/a&gt;)\s*</p>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly string[] AllowedTags =
        [
            "p",
            "h2",
            "h3",
            "h4",
            "ul",
            "ol",
            "li",
            "a",
            "strong",
            "em",
            "u",
            "blockquote",
            "br",
            "code",
            "pre",
            "img",
            "figure",
            "figcaption"
        ];

        private static readonly string[] AllowedAttributes =
        [
            "href",
            "title",
            "src",
            "alt",
            "width",
            "height",
            "class",
            "id",
            "target",
            "rel"
        ];

        private static readonly string[] AllowedSchemes =
        [
            "https"
        ];

        public string Sanitize(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var sanitizer = CreateSanitizer();
            var normalizedHtml = NormalizeEscapedAnchorParagraphs(html);

            return sanitizer.Sanitize(normalizedHtml);
        }

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer();

            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedSchemes.Clear();

            foreach (var tag in AllowedTags)
            {
                sanitizer.AllowedTags.Add(tag);
            }

            foreach (var attribute in AllowedAttributes)
            {
                sanitizer.AllowedAttributes.Add(attribute);
            }

            foreach (var scheme in AllowedSchemes)
            {
                sanitizer.AllowedSchemes.Add(scheme);
            }

            return sanitizer;
        }

        private static string NormalizeEscapedAnchorParagraphs(string html)
        {
            return EscapedAnchorParagraphRegex.Replace(
                html,
                match => WebUtility.HtmlDecode(match.Groups["anchor"].Value));
        }
    }
}
