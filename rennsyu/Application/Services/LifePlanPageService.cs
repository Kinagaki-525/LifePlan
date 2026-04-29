using rennsyu.Application.Interfaces;
using rennsyu.Application.Mappers;
using rennsyu.Application.Results;
using rennsyu.Application.Validators;
using rennsyu.Domain.ReferenceData;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Services
{
    public class LifePlanPageService : ILifePlanPageService
    {
        public LifePlanViewModel CreateInitialPage()
        {
            return PopulatePageDefaults(new LifePlanViewModel
            {
                Family = new FamilyInputViewModel
                {
                    Children = LifePlanPageMapper.CreateChildInputs()
                },
                LifeEvents = new LifeEventInputViewModel
                {
                    EducationPlans = LifePlanPageMapper.CreateEducationPlans()
                }
            });
        }

        public LifePlanSubmitResult Submit(LifePlanViewModel input, bool hasBindingErrors)
        {
            var page = PopulatePageDefaults(input);
            var errors = LifePlanInputValidator.Validate(page);
            var isValid = !hasBindingErrors && errors.Count == 0;
            page.IsSubmitted = isValid;

            return new LifePlanSubmitResult
            {
                Page = page,
                IsValid = isValid,
                Errors = errors,
                Data = isValid ? LifePlanPageMapper.ToLifePlanData(page) : null
            };
        }

        private static LifePlanViewModel PopulatePageDefaults(LifePlanViewModel page)
        {
            page.Family ??= new FamilyInputViewModel();
            page.LifeEvents ??= new LifeEventInputViewModel();
            page.Savings ??= new SavingsInputViewModel();
            page.IncomeExpense ??= new IncomeExpenseInputViewModel();
            page.IncomeExpense.HusbandIncome ??= new PersonIncomeInputViewModel();
            page.IncomeExpense.WifeIncome ??= new PersonIncomeInputViewModel();

            page.Family.Children = MergeChildInputs(page.Family.Children);

            if (page.LifeEvents.EducationPlans.Count == 0)
            {
                page.LifeEvents.EducationPlans = LifePlanPageMapper.CreateEducationPlans();
            }

            page.ChildAgeOptions = LifePlanPageMapper.CreateChildAgeOptions();
            page.EducationOptions = LifePlanPageMapper.ToEducationOptions(EducationCostMaster.Entries);
            page.PensionReferenceOptions = LifePlanPageMapper.ToPensionReferenceOptions(PensionReferenceData.All);
            page.OccupationOptions = LifePlanPageMapper.ToOccupationOptions(OccupationReferenceData.All);

            return page;
        }

        private static List<ChildInputViewModel> MergeChildInputs(List<ChildInputViewModel>? postedChildren)
        {
            var defaults = LifePlanPageMapper.CreateChildInputs();
            postedChildren ??= [];

            for (var index = 0; index < defaults.Count; index++)
            {
                if (index < postedChildren.Count)
                {
                    defaults[index].Age = postedChildren[index].Age;
                }
            }

            return defaults;
        }

    }
}
