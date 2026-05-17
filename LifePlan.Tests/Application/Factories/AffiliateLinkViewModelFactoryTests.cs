using LifePlan.Application.Factories;
using LifePlan.Application.Options;

namespace LifePlan.Tests.Application.Factories
{
    public class AffiliateLinkViewModelFactoryTests
    {
        [Fact]
        public void Create_ReturnsConfiguredLink()
        {
            var link = AffiliateLinkViewModelFactory.Create(new AffiliateLinkOptions
            {
                Url = " https://example.com/fp ",
                TrackingPixelUrl = " https://example.com/pixel.gif ",
                Rel = " sponsored nofollow noopener noreferrer ",
                IsSponsored = true
            });

            Assert.NotNull(link);
            Assert.Equal("https://example.com/fp", link.Url);
            Assert.Equal("https://example.com/pixel.gif", link.TrackingPixelUrl);
            Assert.Equal("sponsored nofollow noopener noreferrer", link.Rel);
            Assert.True(link.IsSponsored);
            Assert.True(link.HasTrackingPixel);
        }

        [Fact]
        public void Create_ReturnsNullWhenUrlIsMissing()
        {
            var link = AffiliateLinkViewModelFactory.Create(new AffiliateLinkOptions
            {
                Url = " ",
                TrackingPixelUrl = "https://example.com/pixel.gif",
                Rel = "sponsored nofollow noopener noreferrer",
                IsSponsored = true
            });

            Assert.Null(link);
        }

        [Fact]
        public void Create_UsesDefaultRelWhenRelIsMissing()
        {
            var link = AffiliateLinkViewModelFactory.Create(new AffiliateLinkOptions
            {
                Url = "https://example.com/fp"
            });

            Assert.NotNull(link);
            Assert.Equal("sponsored nofollow noopener noreferrer", link.Rel);
        }

        [Fact]
        public void Create_UsesSponsoredByDefaultWhenIsSponsoredIsMissing()
        {
            var link = AffiliateLinkViewModelFactory.Create(new AffiliateLinkOptions
            {
                Url = "https://example.com/fp"
            });

            Assert.NotNull(link);
            Assert.True(link.IsSponsored);
        }
    }
}
