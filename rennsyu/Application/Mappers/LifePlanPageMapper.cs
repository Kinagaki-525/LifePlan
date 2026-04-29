using rennsyu.Domain.ReferenceData;
using rennsyu.Domain.Rules;
using rennsyu.Domain.Entities;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Mappers
{
    public static class LifePlanPageMapper
    {
        public static List<ChildInputViewModel> CreateChildInputs()
        {
            return CreateChildLabels()
                .Select(label => new ChildInputViewModel { Label = label })
                .ToList();
        }

        public static List<ChildEducationInputViewModel> CreateEducationPlans()
        {
            return CreateChildLabels()
                .Select(label => new ChildEducationInputViewModel { Label = label })
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> CreateChildAgeOptions()
        {
            var options = new List<SelectOptionViewModel>
            {
                new(string.Empty, "-（なし）")
            };

            for (var age = AgeRules.MinChildAge; age <= AgeRules.MaxChildAge; age++)
            {
                options.Add(new SelectOptionViewModel(age.ToString(), age.ToString()));
            }

            return options;
        }

        public static IReadOnlyList<SelectOptionViewModel> ToEducationOptions(IReadOnlyList<EducationCostEntry> entries)
        {
            return entries
                .Select(entry => new SelectOptionViewModel(entry.Value, $"{entry.Stage}：{entry.Type}"))
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> ToEducationOptionsByStage(
            IReadOnlyList<EducationCostEntry> entries,
            string stage)
        {
            return entries
                .Where(entry => entry.Stage == stage)
                .Select(entry => new SelectOptionViewModel(entry.Value, entry.Type))
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> ToPensionReferenceOptions(IReadOnlyList<PensionReferenceEntry> references)
        {
            return references
                .Select(reference => new SelectOptionViewModel(reference.Value, reference.DisplayName))
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> ToOccupationOptions(IReadOnlyList<OccupationEntry> occupations)
        {
            return occupations
                .Select(occupation => new SelectOptionViewModel(occupation.Value, occupation.DisplayName))
                .ToList();
        }

        public static LifePlanData ToLifePlanData(LifePlanViewModel model)
        {
            return new LifePlanData
            {
                Family = new FamilyData
                {
                    HusbandAge = model.Family.HusbandAge.GetValueOrDefault(),
                    WifeAge = model.Family.WifeAge.GetValueOrDefault(),
                    Children = model.Family.Children
                        .Where(child => child.Age.HasValue)
                        .Select(child => new ChildData { Age = child.Age })
                        .ToList()
                },
                LifeEvents = new LifeEventData
                {
                    Marriage = ToMarriageEventData(model.LifeEvents.Marriage),
                    Housing = ToHousingEventData(model.LifeEvents.Housing),
                    Car = ToCarEventData(model.LifeEvents.Car),
                    EducationPlans = model.LifeEvents.EducationPlans
                        .Select(ToChildEducationData)
                        .ToList(),
                    TravelOther = ToTravelOtherEventData(model.LifeEvents.TravelOther)
                },
                Assets = new AssetData
                {
                    CurrentFinancialAssetsYen = ToYen(model.Savings.CurrentFinancialAssetsManYen),
                    ExpectedAnnualReturnRatePercent = model.Savings.ExpectedAnnualReturnRatePercent
                },
                IncomeExpense = new IncomeExpenseData
                {
                    HusbandIncome = ToPersonIncomeData(model.IncomeExpense.HusbandIncome),
                    WifeIncome = ToPersonIncomeData(model.IncomeExpense.WifeIncome),
                    Expenses = ToExpenseData(model.IncomeExpense.Expenses)
                }
            };
        }

        private static IReadOnlyList<string> CreateChildLabels()
        {
            return
            [
                "第1子",
                "第2子",
                "第3子",
                "第4子"
            ];
        }

        private static PersonIncomeData ToPersonIncomeData(PersonIncomeInputViewModel input)
        {
            return new PersonIncomeData
            {
                OccupationValue = input.OccupationValue,
                AnnualIncomeYen = ToYen(input.AnnualIncomeManYen),
                AnnualIncomeChangeRatePercent = input.AnnualIncomeChangeRatePercent,
                WorkStartAge = input.WorkStartAge,
                WorkEndAge = input.WorkEndAge,
                RetirementAllowanceYen = ToYen(input.RetirementAllowanceManYen),
                AnnualPensionYen = ToYen(input.AnnualPensionManYen),
                PensionStartAge = input.PensionStartAge
            };
        }

        private static MarriageEventData ToMarriageEventData(MarriageEventInputViewModel input)
        {
            return new MarriageEventData
            {
                CostYen = ToYen(input.CostManYen),
                HusbandAge = input.HusbandAge
            };
        }

        private static HousingEventData ToHousingEventData(HousingEventInputViewModel input)
        {
            return new HousingEventData
            {
                DownPaymentYen = ToYen(input.DownPaymentManYen),
                BorrowingAmountYen = ToYen(input.BorrowingAmountManYen),
                PurchaseHusbandAge = input.PurchaseHusbandAge,
                LoanYears = input.LoanYears,
                InterestRatePercent = input.InterestRatePercent
            };
        }

        private static CarEventData ToCarEventData(CarEventInputViewModel input)
        {
            return new CarEventData
            {
                PurchaseAmountYen = ToYen(input.PurchaseAmountManYen),
                FirstPurchaseHusbandAge = input.FirstPurchaseHusbandAge,
                ReplacementIntervalYears = input.ReplacementIntervalYears
            };
        }

        private static ChildEducationData ToChildEducationData(ChildEducationInputViewModel input)
        {
            return new ChildEducationData
            {
                NurseryOptionValue = input.NurseryOptionValue,
                KindergartenOptionValue = input.KindergartenOptionValue,
                ElementarySchoolOptionValue = input.ElementarySchoolOptionValue,
                JuniorHighSchoolOptionValue = input.JuniorHighSchoolOptionValue,
                HighSchoolOptionValue = input.HighSchoolOptionValue,
                UniversityOptionValue = input.UniversityOptionValue,
                GraduateSchoolOptionValue = input.GraduateSchoolOptionValue
            };
        }

        private static TravelOtherEventData ToTravelOtherEventData(TravelOtherEventInputViewModel input)
        {
            return new TravelOtherEventData
            {
                AnnualCostYen = ToYen(input.AnnualCostManYen),
                StartHusbandAge = input.StartHusbandAge,
                EndHusbandAge = input.EndHusbandAge
            };
        }

        private static ExpenseData ToExpenseData(ExpenseInputViewModel input)
        {
            return new ExpenseData
            {
                MonthlyBasicLivingCostYen = ToYen(input.MonthlyBasicLivingCostManYen),
                InflationRatePercent = input.InflationRatePercent,
                MonthlyRentYen = ToYen(input.MonthlyRentManYen),
                OtherAnnualCostYen = ToYen(input.OtherAnnualCostManYen)
            };
        }

        private static long? ToYen(decimal? manYen)
        {
            return manYen.HasValue
                ? decimal.ToInt64(decimal.Round(manYen.Value * 10000m, 0, MidpointRounding.AwayFromZero))
                : null;
        }
    }
}
