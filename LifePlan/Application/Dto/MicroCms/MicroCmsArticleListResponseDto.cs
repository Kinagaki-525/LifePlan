using System.Text.Json.Serialization;

namespace LifePlan.Application.Dto.MicroCms
{
    public class MicroCmsArticleListResponseDto
    {
        [JsonPropertyName("contents")]
        public List<MicroCmsArticleDto> Contents { get; set; } = [];

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }
    }
}
