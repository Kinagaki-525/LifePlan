using rennsyu.Application.Interfaces;
using rennsyu.Application.Mappers;
using rennsyu.Application.Normalizers;
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
            var normalizedInput = isValid ? LifePlanInputNormalizer.Normalize(page) : null;
            page.IsSubmitted = isValid;

            return new LifePlanSubmitResult
            {
                Page = page,
                IsValid = isValid,
                Errors = errors,
                Data = normalizedInput is null ? null : LifePlanPageMapper.ToLifePlanData(normalizedInput)
            };
        }

        private static LifePlanViewModel PopulatePageDefaults(LifePlanViewModel page)
        {
            page.Family ??= new FamilyInputViewModel();
            page.LifeEvents ??= new LifeEventInputViewModel();
            page.LifeEvents.Marriage ??= new MarriageEventInputViewModel();
            page.LifeEvents.Housing ??= new HousingEventInputViewModel();
            page.LifeEvents.Car ??= new CarEventInputViewModel();
            page.LifeEvents.TravelOther ??= new TravelOtherEventInputViewModel();
            page.Savings ??= new SavingsInputViewModel();
            page.IncomeExpense ??= new IncomeExpenseInputViewModel();
            page.IncomeExpense.HusbandIncome ??= new PersonIncomeInputViewModel();
            page.IncomeExpense.WifeIncome ??= new PersonIncomeInputViewModel();
            page.IncomeExpense.Expenses ??= new ExpenseInputViewModel();

            page.Family.Children = MergeChildInputs(page.Family.Children);
            page.LifeEvents.EducationPlans = MergeEducationPlans(page.LifeEvents.EducationPlans);

            page.ChildAgeOptions = LifePlanPageMapper.CreateChildAgeOptions();
            page.EducationOptions = LifePlanPageMapper.ToEducationOptions(EducationCostMaster.Entries);
            page.NurseryEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "保育園");
            page.KindergartenEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "幼稚園・保育園");
            page.ElementarySchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "小学校");
            page.JuniorHighSchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "中学校");
            page.HighSchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "高校");
            page.UniversityEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "大学");
            page.GraduateSchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "大学院");
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

        private static List<ChildEducationInputViewModel> MergeEducationPlans(List<ChildEducationInputViewModel>? postedPlans)
        {
            var defaults = LifePlanPageMapper.CreateEducationPlans();
            postedPlans ??= [];

            for (var index = 0; index < defaults.Count; index++)
            {
                if (index < postedPlans.Count)
                {
                    defaults[index].NurseryOptionValue = postedPlans[index].NurseryOptionValue;
                    defaults[index].KindergartenOptionValue = postedPlans[index].KindergartenOptionValue;
                    defaults[index].ElementarySchoolOptionValue = postedPlans[index].ElementarySchoolOptionValue;
                    defaults[index].JuniorHighSchoolOptionValue = postedPlans[index].JuniorHighSchoolOptionValue;
                    defaults[index].HighSchoolOptionValue = postedPlans[index].HighSchoolOptionValue;
                    defaults[index].UniversityOptionValue = postedPlans[index].UniversityOptionValue;
                    defaults[index].GraduateSchoolOptionValue = postedPlans[index].GraduateSchoolOptionValue;
                }
            }

            return defaults;
        }

    }
}
