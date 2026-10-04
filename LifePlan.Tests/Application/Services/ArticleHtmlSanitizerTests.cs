using LifePlan.Application.Services;

namespace LifePlan.Tests.Application.Services;

public class ArticleHtmlSanitizerTests
{
    [Fact]
    public void Sanitize_RemovesExecutableHtml()
    {
        // Arrange
        var sanitizer = new ArticleHtmlSanitizer();
        const string html = """
            <p>本文</p>
            <script>alert(1)</script>
            <iframe src="https://example.com"></iframe>
            <img src="https://example.com/image.jpg" alt="画像" onload="alert(1)">
            <a href="/Articles/sample" title="記事">相対リンク</a>
            <a href="javascript:alert(1)" onclick="alert(1)">危険なリンク</a>
            """;

        // Act
        var result = sanitizer.Sanitize(html);

        // Assert
        Assert.Contains("<p>本文</p>", result);
        Assert.Contains("https://example.com/image.jpg", result);
        Assert.Contains("/Articles/sample", result);
        Assert.DoesNotContain("<script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<iframe", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onload", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_AllowsArticleLinkAttributes()
    {
        // Arrange
        var sanitizer = new ArticleHtmlSanitizer();
        const string html = """
            <a href="https://futari-kakei.com/simulation"
               class="btn-primary"
               id="btn-simulation"
               target="_blank"
               rel="sponsored nofollow noopener noreferrer">
               ライフプランシミュレーションを無料で試す
            </a>
            """;

        // Act
        var result = sanitizer.Sanitize(html);

        // Assert
        Assert.Contains("href=\"https://futari-kakei.com/simulation\"", result);
        Assert.Contains("class=\"btn-primary\"", result);
        Assert.Contains("id=\"btn-simulation\"", result);
        Assert.Contains("target=\"_blank\"", result);
        Assert.Contains("rel=\"sponsored nofollow noopener noreferrer\"", result);
    }

    [Fact]
    public void Sanitize_NormalizesEscapedAnchorParagraph()
    {
        // Arrange
        var sanitizer = new ArticleHtmlSanitizer();
        const string html = """
            <p>&lt;a href=&quot;https://futari-kakei.com/simulation&quot;
             class=&quot;btn-primary&quot;
             id=&quot;btn-simulation&quot;&gt;
               ライフプランシミュレーションを無料で試す
            &lt;/a&gt;</p>
            """;

        // Act
        var result = sanitizer.Sanitize(html);

        // Assert
        Assert.Contains("<a", result);
        Assert.Contains("href=\"https://futari-kakei.com/simulation\"", result);
        Assert.Contains("class=\"btn-primary\"", result);
        Assert.Contains("id=\"btn-simulation\"", result);
        Assert.DoesNotContain("&lt;a", result);
    }

    [Fact]
    public void Sanitize_DoesNotNormalizeEscapedAnchorInCodeBlock()
    {
        // Arrange
        var sanitizer = new ArticleHtmlSanitizer();
        const string html = """
            <pre><code>&lt;a href=&quot;https://futari-kakei.com/simulation&quot;&gt;リンク&lt;/a&gt;</code></pre>
            """;

        // Act
        var result = sanitizer.Sanitize(html);

        // Assert
        Assert.Contains("&lt;a", result);
        Assert.DoesNotContain("<a href=", result);
    }
}
