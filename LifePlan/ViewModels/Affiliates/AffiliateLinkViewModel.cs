namespace LifePlan.ViewModels.Affiliates
{
    public class AffiliateLinkViewModel
    {
        public string Url { get; init; } = string.Empty;

        public string? TrackingPixelUrl { get; init; }

        public string Rel { get; init; } = "sponsored nofollow noopener noreferrer";

        public bool IsSponsored { get; init; }

        public bool HasTrackingPixel => !string.IsNullOrWhiteSpace(TrackingPixelUrl);
    }
}
