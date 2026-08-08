using System.Net.Http.Json;
using System.Text.Json;
using LifePlan.Application.Dto.MicroCms;
using LifePlan.Application.Interfaces;
using LifePlan.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace LifePlan.Infrastructure.Repositories
{
    public class MicroCmsArticleRepository : IArticleRepository
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient httpClient;
        private readonly MicroCmsOptions options;

        public MicroCmsArticleRepository(HttpClient httpClient, IOptions<MicroCmsOptions> options)
        {
            this.httpClient = httpClient;
            this.options = options.Value;
        }

        public async Task<MicroCmsArticleListResponseDto> GetArticlesAsync(
            int limit,
            int offset,
            string? categoryName,
            CancellationToken cancellationToken)
        {
            EnsureConfigured();
            EnsureBaseAddress();

            using var request = new HttpRequestMessage(HttpMethod.Get, CreateRequestUri(limit, offset, categoryName));
            request.Headers.Add("X-MICROCMS-API-KEY", options.ApiKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var articles = await response.Content.ReadFromJsonAsync<MicroCmsArticleListResponseDto>(
                JsonOptions,
                cancellationToken);

            return articles ?? new MicroCmsArticleListResponseDto();
        }

        public async Task<MicroCmsArticleDto?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            EnsureConfigured();
            EnsureBaseAddress();

            using var request = new HttpRequestMessage(HttpMethod.Get, CreateDetailRequestUri(slug));
            request.Headers.Add("X-MICROCMS-API-KEY", options.ApiKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var articles = await response.Content.ReadFromJsonAsync<MicroCmsArticleListResponseDto>(
                JsonOptions,
                cancellationToken);

            return articles?.Contents.FirstOrDefault();
        }

        private string CreateRequestUri(int limit, int offset, string? categoryName)
        {
            var query = new List<KeyValuePair<string, string>>
            {
                new("limit", limit.ToString()),
                new("offset", offset.ToString()),
                new("orders", "-publishedDate,-publishedAt")
            };

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query.Add(new KeyValuePair<string, string>("filters", $"category[contains]{categoryName.Trim()}"));
            }

            return $"api/v1/{GetEndpoint()}?{CreateQueryString(query)}";
        }

        private string CreateDetailRequestUri(string slug)
        {
            var query = new List<KeyValuePair<string, string>>
            {
                new("filters", $"slug[equals]{slug.Trim()}"),
                new("limit", "1")
            };

            return $"api/v1/{GetEndpoint()}?{CreateQueryString(query)}";
        }

        private static string CreateQueryString(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            return string.Join("&", parameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));
        }

        private string GetEndpoint()
        {
            return string.IsNullOrWhiteSpace(options.ArticlesEndpoint)
                ? "articles"
                : options.ArticlesEndpoint.Trim().Trim('/');
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(options.ServiceDomain))
            {
                throw new InvalidOperationException("MicroCMS service domain is not configured.");
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("MicroCMS API key is not configured.");
            }
        }

        private void EnsureBaseAddress()
        {
            httpClient.BaseAddress ??= new Uri($"https://{options.ServiceDomain!.Trim()}.microcms.io/");
        }
    }
}
