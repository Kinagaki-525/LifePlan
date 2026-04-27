using rennsyu.Application.Interfaces;
using rennsyu.Application.Mappers;
using rennsyu.Domain.ReferenceData;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Services
{
    public class LifePlanPageService : ILifePlanPageService
    {
        public LifePlanViewModel CreateInitialPage()
        {
            return new LifePlanViewModel
            {
                Family = new FamilyInputViewModel
                {
                    Children = LifePlanPageMapper.CreateChildInputs()
                },
                LifeEvents = new LifeEventInputViewModel
                {
                    EducationPlans = LifePlanPageMapper.CreateEducationPlans()
                },

                ChildAgeOptions = LifePlanPageMapper.CreateChildAgeOptions(),
                EducationOptions = LifePlanPageMapper.ToEducationOptions(EducationCostMaster.Entries),
                PensionReferenceOptions = LifePlanPageMapper.ToPensionReferenceOptions(PensionReferenceData.All),
                OccupationOptions = LifePlanPageMapper.CreateOccupationOptions()
            };
        }
    }
}
