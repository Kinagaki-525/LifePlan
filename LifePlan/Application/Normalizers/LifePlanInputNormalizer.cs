using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Normalizers
{
    public static class LifePlanInputNormalizer
    {
        public static LifePlanViewModel Normalize(LifePlanViewModel input)
        {
            return new LifePlanViewModel
            {
                Family = input.Family,
                LifeEvents = NormalizeLifeEvents(input),
                Savings = input.Savings,
                IncomeExpense = NormalizeIncomeExpense(input)
            };
        }

        private static LifeEventInputViewModel NormalizeLifeEvents(LifePlanViewModel input)
        {
            return new LifeEventInputViewModel
            {
                Marriage = input.LifeEvents.Marriage,
                Housing = NormalizeHousing(input.LifeEvents.Housing),
                Car = NormalizeCar(input.LifeEvents.Car),
                EducationPlans = NormalizeEducationPlans(input.Family.Children, input.LifeEvents.EducationPlans),
                TravelOther = input.LifeEvents.TravelOther
            };
        }

        private static HousingEventInputViewModel NormalizeHousing(HousingEventInputViewModel input)
        {
            if (!input.PurchaseHusbandAge.HasValue)
            {
                return new HousingEventInputViewModel();
            }

            return input;
        }

        private static CarEventInputViewModel NormalizeCar(CarEventInputViewModel input)
        {
            if (!input.FirstPurchaseHusbandAge.HasValue)
            {
                return new CarEventInputViewModel
                {
                    PurchaseAmountManYen = input.PurchaseAmountManYen
                };
            }

            return input;
        }

        private static List<ChildEducationInputViewModel> NormalizeEducationPlans(
            IReadOnlyList<ChildInputViewModel> children,
            IReadOnlyList<ChildEducationInputViewModel> educationPlans)
        {
            var normalized = new List<ChildEducationInputViewModel>();

            for (var index = 0; index < educationPlans.Count; index++)
            {
                if (index >= children.Count || !children[index].Age.HasValue)
                {
                    normalized.Add(new ChildEducationInputViewModel
                    {
                        Label = educationPlans[index].Label
                    });
                    continue;
                }

                normalized.Add(educationPlans[index]);
            }

            return normalized;
        }

        private static IncomeExpenseInputViewModel NormalizeIncomeExpense(LifePlanViewModel input)
        {
            return new IncomeExpenseInputViewModel
            {
                HusbandIncome = NormalizePersonIncome(input.IncomeExpense.HusbandIncome),
                WifeIncome = NormalizePersonIncome(input.IncomeExpense.WifeIncome),
                Expenses = input.IncomeExpense.Expenses
            };
        }

        private static PersonIncomeInputViewModel NormalizePersonIncome(PersonIncomeInputViewModel input)
        {
            return new PersonIncomeInputViewModel
            {
                OccupationValue = input.OccupationValue,
                AnnualIncomeManYen = input.AnnualIncomeManYen,
                AnnualIncomeChangeRatePercent = input.AnnualIncomeChangeRatePercent,
                WorkStartAge = input.WorkStartAge,
                WorkEndAge = input.WorkEndAge,
                RetirementAllowanceManYen = input.RetirementAllowanceManYen,
                AnnualPensionManYen = input.AnnualPensionManYen,
                PensionStartAge = input.PensionStartAge
            };
        }
    }
}
