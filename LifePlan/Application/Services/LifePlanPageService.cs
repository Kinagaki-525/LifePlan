using LifePlan.Application.Interfaces;
using LifePlan.Application.Mappers;
using LifePlan.Application.Normalizers;
using LifePlan.Application.Results;
using LifePlan.Application.Validators;
using LifePlan.Domain.Logic;
using LifePlan.Domain.ReferenceData;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Services
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
            var data = normalizedInput is null ? null : LifePlanPageMapper.ToLifePlanData(normalizedInput);
            var calculationResult = data is null ? null : new LifePlanCalculator().Calculate(data, DateTime.Today.Year);
            page.IsSubmitted = isValid;

            return new LifePlanSubmitResult
            {
                Page = page,
                IsValid = isValid,
                Errors = errors,
                CalculationResult = calculationResult
            };
        }

        private static LifePlanViewModel PopulatePageDefaults(LifePlanViewModel page)
        {
            EnsureInputModels(page);

            page.Family.Children = MergeChildInputs(page.Family.Children);
            page.LifeEvents.EducationPlans = MergeEducationPlans(page.LifeEvents.EducationPlans);

            PopulateSelectOptions(page);

            return page;
        }

        private static void EnsureInputModels(LifePlanViewModel page)
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
        }

        private static void PopulateSelectOptions(LifePlanViewModel page)
        {
            page.ChildAgeOptions = LifePlanPageMapper.CreateChildAgeOptions();
            page.EducationOptions = LifePlanPageMapper.ToEducationOptions(EducationCostMaster.Entries);
            page.NurseryEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "保育園");
            page.KindergartenEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "幼稚園・保育園");
            page.ElementarySchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "小学校");
            page.JuniorHighSchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "中学校");
            page.HighSchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "高校");
            page.UniversityEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "大学");
            page.GraduateSchoolEducationOptions = LifePlanPageMapper.ToEducationOptionsByStage(EducationCostMaster.Entries, "大学院");
            page.OccupationOptions = LifePlanPageMapper.ToOccupationOptions(OccupationReferenceData.All);
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
                    CopyEducationPlanValues(defaults[index], postedPlans[index]);
                }
            }

            return defaults;
        }

        private static void CopyEducationPlanValues(
            ChildEducationInputViewModel target,
            ChildEducationInputViewModel source)
        {
            target.NurseryOptionValue = source.NurseryOptionValue;
            target.KindergartenOptionValue = source.KindergartenOptionValue;
            target.ElementarySchoolOptionValue = source.ElementarySchoolOptionValue;
            target.JuniorHighSchoolOptionValue = source.JuniorHighSchoolOptionValue;
            target.HighSchoolOptionValue = source.HighSchoolOptionValue;
            target.UniversityOptionValue = source.UniversityOptionValue;
            target.GraduateSchoolOptionValue = source.GraduateSchoolOptionValue;
        }

    }
}
