using System.Net;
using System.Text;
using LifePlan.Infrastructure.Options;
using LifePlan.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace LifePlan.Tests.Infrastructure.Repositories;

public class MicroCmsArticleRepositoryTests
{
    [Fact]
    public async Task GetArticlesAsync_UsesPublishedDateOrder()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var repository = new MicroCmsArticleRepository(
            new HttpClient(handler),
            Options.Create(new MicroCmsOptions
            {
                ServiceDomain = "example-service",
                ApiKey = "api-key",
                ArticlesEndpoint = "articles"
            }));

        // Act
        await repository.GetArticlesAsync(
            limit: 6,
            offset: 0,
            categoryName: "家計管理",
            CancellationToken.None);

        // Assert
        Assert.NotNull(handler.RequestUri);
        Assert.Contains("orders=-publishedDate%2C-publishedAt", handler.RequestUri.Query);
        Assert.Contains("filters=category%5Bcontains%5D%E5%AE%B6%E8%A8%88%E7%AE%A1%E7%90%86", handler.RequestUri.Query);
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"contents\":[],\"totalCount\":0,\"limit\":6,\"offset\":0}",
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }
}
