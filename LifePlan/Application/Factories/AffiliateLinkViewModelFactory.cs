using LifePlan.Application.Options;
using LifePlan.ViewModels.Affiliates;

namespace LifePlan.Application.Factories
{
    public static class AffiliateLinkViewModelFactory
    {
        private const string DefaultRel = "sponsored nofollow noopener noreferrer";

        public static AffiliateLinkViewModel? Create(AffiliateLinkOptions link)
        {
            if (string.IsNullOrWhiteSpace(link.Url))
            {
                return null;
            }

            return new AffiliateLinkViewModel
            {
                Url = link.Url.Trim(),
                TrackingPixelUrl = string.IsNullOrWhiteSpace(link.TrackingPixelUrl)
                    ? null
                    : link.TrackingPixelUrl.Trim(),
                Rel = string.IsNullOrWhiteSpace(link.Rel)
                    ? DefaultRel
                    : link.Rel.Trim(),
                IsSponsored = link.IsSponsored ?? true
            };
        }
    }
}
