using LifePlan.Application.Factories;
using LifePlan.Application.Interfaces;
using LifePlan.Application.Options;
using LifePlan.ViewModels.Affiliates;
using Microsoft.Extensions.Options;

namespace LifePlan.Application.Services
{
    public class AffiliateLinkService : IAffiliateLinkService
    {
        private readonly AffiliateLinksOptions options;

        public AffiliateLinkService(IOptions<AffiliateLinksOptions> options)
        {
            this.options = options.Value;
        }

        public AffiliateLinkViewModel? GetFpConsultationLink()
        {
            return AffiliateLinkViewModelFactory.Create(options.FpConsultation);
        }
    }
}
