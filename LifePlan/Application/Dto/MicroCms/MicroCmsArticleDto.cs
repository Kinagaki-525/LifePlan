using System.Text.Json;
using System.Text.Json.Serialization;

namespace LifePlan.Application.Dto.MicroCms
{
    public class MicroCmsArticleDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTimeOffset? PublishedAt { get; set; }

        [JsonPropertyName("publishedDate")]
        public DateTimeOffset? PublishedDate { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("thumbnail")]
        public MicroCmsImageDto? Thumbnail { get; set; }

        [JsonPropertyName("category")]
        public JsonElement? Category { get; set; }

        [JsonPropertyName("tags")]
        public string? Tags { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("metaDescription")]
        public string? MetaDescription { get; set; }
    }
}
