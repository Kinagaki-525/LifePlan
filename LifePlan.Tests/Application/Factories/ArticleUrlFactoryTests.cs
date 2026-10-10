using LifePlan.Application.Factories;

namespace LifePlan.Tests.Application.Factories
{
    public class ArticleUrlFactoryTests
    {
        [Fact]
        public void CreateListUrl_ReturnsArticlesPathWithoutQuery()
        {
            Assert.Equal("/Articles", ArticleUrlFactory.CreateListUrl());
        }

        [Fact]
        public void CreateListUrl_OmitsFirstPage()
        {
            Assert.Equal("/Articles?category=money", ArticleUrlFactory.CreateListUrl("money", 1));
        }

        [Fact]
        public void CreateListUrl_AddsCategoryAndPage()
        {
            Assert.Equal("/Articles?category=money&page=2", ArticleUrlFactory.CreateListUrl("money", 2));
        }

        [Fact]
        public void CreateDetailUrl_TrimsAndEscapesSlug()
        {
            Assert.Equal(
                "/Articles/life-plan-simulation-dual-income-couples",
                ArticleUrlFactory.CreateDetailUrl(" life-plan-simulation-dual-income-couples "));
            Assert.Equal("/Articles/a%2Fb%20c", ArticleUrlFactory.CreateDetailUrl("a/b c"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CreateDetailUrl_ReturnsArticlesPathWhenSlugIsMissing(string? slug)
        {
            Assert.Equal("/Articles", ArticleUrlFactory.CreateDetailUrl(slug));
        }
    }
}
