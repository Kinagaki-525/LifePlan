using System.Text.Json.Serialization;

namespace LifePlan.Application.Dto.MicroCms
{
    public class MicroCmsImageDto
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }
    }
}
