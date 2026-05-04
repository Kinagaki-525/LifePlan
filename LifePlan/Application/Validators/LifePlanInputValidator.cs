using LifePlan.Application.Results;
using LifePlan.Domain.ReferenceData;
using LifePlan.Domain.Rules;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Validators
{
    public static class LifePlanInputValidator
    {
        public static IReadOnlyList<LifePlanValidationError> Validate(LifePlanViewModel page)
        {
            var errors = new List<LifePlanValidationError>();

            ValidateRequiredAdultAge(errors, "Family.HusbandAge", page.Family.HusbandAge, "夫の年齢");
            ValidateRequiredAdultAge(errors, "Family.WifeAge", page.Family.WifeAge, "妻の年齢");
            ValidateChildren(errors, page.Family.Children);
            ValidateLifeEvents(errors, page);
            ValidateSavings(errors, page.Savings);
            ValidatePersonIncome(errors, "IncomeExpense.HusbandIncome", page.IncomeExpense.HusbandIncome, "夫");
            ValidatePersonIncome(errors, "IncomeExpense.WifeIncome", page.IncomeExpense.WifeIncome, "妻");
            ValidateExpenses(errors, page.IncomeExpense.Expenses);

            return errors;
        }

        private static void ValidateChildren(List<LifePlanValidationError> errors, IReadOnlyList<ChildInputViewModel> children)
        {
            for (var index = 0; index < children.Count; index++)
            {
                var age = children[index].Age;

                if (age.HasValue && !AgeRules.IsChildAgeInRange(age.Value))
                {
                    errors.Add(new LifePlanValidationError(
                        $"Family.Children[{index}].Age",
                        LifePlanValidationMessages.ChildAgeRange()));
                }
            }
        }

        private static void ValidateSavings(
            List<LifePlanValidationError> errors,
            SavingsInputViewModel savings)
        {
            ValidateNonNegative(errors, "Savings.CurrentFinancialAssetsManYen", savings.CurrentFinancialAssetsManYen, "現在の金融資産残高");

            if (savings.ExpectedAnnualReturnRatePercent.HasValue &&
                !RateRules.IsExpectedAnnualReturnRateInRange(savings.ExpectedAnnualReturnRatePercent.Value))
            {
                errors.Add(new LifePlanValidationError(
                    "Savings.ExpectedAnnualReturnRatePercent",
                    LifePlanValidationMessages.ExpectedAnnualReturnRateRange()));
            }
        }

        private static void ValidateLifeEvents(List<LifePlanValidationError> errors, LifePlanViewModel page)
        {
            ValidateMarriage(errors, page.LifeEvents.Marriage);
            ValidateHousing(errors, page.LifeEvents.Housing);
            ValidateCar(errors, page.LifeEvents.Car);
            ValidateEducationPlans(errors, page.Family.Children, page.LifeEvents.EducationPlans);
            ValidateTravelOther(errors, page.LifeEvents.TravelOther);
        }

        private static void ValidateMarriage(List<LifePlanValidationError> errors, MarriageEventInputViewModel marriage)
        {
            ValidateNonNegative(errors, "LifeEvents.Marriage.CostManYen", marriage.CostManYen, "結婚費用");
            ValidateOptionalAdultAge(errors, "LifeEvents.Marriage.HusbandAge", marriage.HusbandAge, "結婚実施年齢");
        }

        private static void ValidateHousing(List<LifePlanValidationError> errors, HousingEventInputViewModel housing)
        {
            if (!housing.PurchaseHusbandAge.HasValue)
            {
                return;
            }

            ValidateOptionalAdultAge(errors, "LifeEvents.Housing.PurchaseHusbandAge", housing.PurchaseHusbandAge, "住宅購入時期");
            ValidateNonNegative(errors, "LifeEvents.Housing.DownPaymentManYen", housing.DownPaymentManYen, "住宅購入の頭金");
            ValidateNonNegative(errors, "LifeEvents.Housing.BorrowingAmountManYen", housing.BorrowingAmountManYen, "住宅購入の借入額");
            ValidatePositive(errors, "LifeEvents.Housing.LoanYears", housing.LoanYears, "住宅ローン年数");

            if (housing.InterestRatePercent.HasValue && housing.InterestRatePercent.Value < RateRules.MinRatePercent)
            {
                errors.Add(new LifePlanValidationError(
                    "LifeEvents.Housing.InterestRatePercent",
                    LifePlanValidationMessages.HousingInterestRateRange()));
            }
        }

        private static void ValidateCar(List<LifePlanValidationError> errors, CarEventInputViewModel car)
        {
            ValidateNonNegative(errors, "LifeEvents.Car.PurchaseAmountManYen", car.PurchaseAmountManYen, "自動車購入額");

            if (!car.FirstPurchaseHusbandAge.HasValue)
            {
                return;
            }

            ValidateOptionalAdultAge(errors, "LifeEvents.Car.FirstPurchaseHusbandAge", car.FirstPurchaseHusbandAge, "自動車の初回購入年齢");
            ValidatePositive(errors, "LifeEvents.Car.ReplacementIntervalYears", car.ReplacementIntervalYears, "自動車の買い替え間隔");
        }

        private static void ValidateEducationPlans(
            List<LifePlanValidationError> errors,
            IReadOnlyList<ChildInputViewModel> children,
            IReadOnlyList<ChildEducationInputViewModel> educationPlans)
        {
            for (var index = 0; index < educationPlans.Count; index++)
            {
                if (index >= children.Count || !children[index].Age.HasValue)
                {
                    continue;
                }

                ValidateEducationOption(errors, $"LifeEvents.EducationPlans[{index}].NurseryOptionValue", educationPlans[index].NurseryOptionValue, "保育園");
                ValidateEducationOption(errors, $"LifeEvents.EducationPlans[{index}].KindergartenOptionValue", educationPlans[index].KindergartenOptionValue, "幼稚園・保育園");
                ValidateEducationOption(errors, $"LifeEvents.EducationPlans[{index}].ElementarySchoolOptionValue", educationPlans[index].ElementarySchoolOptionValue, "小学校");
                ValidateEducationOption(errors, $"LifeEvents.EducationPlans[{index}].JuniorHighSchoolOptionValue", educationPlans[index].JuniorHighSchoolOptionValue, "中学校");
                ValidateEducationOption(errors, $"LifeEvents.EducationPlans[{index}].HighSchoolOptionValue", educationPlans[index].HighSchoolOptionValue, "高校");
                ValidateEducationOption(errors, $"LifeEvents.EducationPlans[{index}].UniversityOptionValue", educationPlans[index].UniversityOptionValue, "大学");
                ValidateEducationOption(errors, $"LifeEvents.EducationPlans[{index}].GraduateSchoolOptionValue", educationPlans[index].GraduateSchoolOptionValue, "大学院");
            }
        }

        private static void ValidateEducationOption(
            List<LifePlanValidationError> errors,
            string key,
            string? value,
            string stage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (EducationCostMaster.Entries.All(entry => entry.Stage != stage || entry.Value != value))
            {
                errors.Add(new LifePlanValidationError(key, LifePlanValidationMessages.EducationDefinedOption(stage)));
            }
        }

        private static void ValidateTravelOther(List<LifePlanValidationError> errors, TravelOtherEventInputViewModel travelOther)
        {
            ValidateNonNegative(errors, "LifeEvents.TravelOther.AnnualCostManYen", travelOther.AnnualCostManYen, "旅行・その他の年間費用");
            ValidateOptionalAdultAge(errors, "LifeEvents.TravelOther.StartHusbandAge", travelOther.StartHusbandAge, "旅行・その他の開始年齢");
            ValidateOptionalAdultAge(errors, "LifeEvents.TravelOther.EndHusbandAge", travelOther.EndHusbandAge, "旅行・その他の終了年齢");

            if (travelOther.StartHusbandAge.HasValue &&
                travelOther.EndHusbandAge.HasValue &&
                travelOther.EndHusbandAge.Value < travelOther.StartHusbandAge.Value)
            {
                errors.Add(new LifePlanValidationError(
                    "LifeEvents.TravelOther.EndHusbandAge",
                    LifePlanValidationMessages.EndAgeMustBeStartAgeOrLater("旅行・その他の終了年齢")));
            }
        }

        private static void ValidateExpenses(List<LifePlanValidationError> errors, ExpenseInputViewModel expenses)
        {
            ValidateNonNegative(errors, "IncomeExpense.Expenses.MonthlyBasicLivingCostManYen", expenses.MonthlyBasicLivingCostManYen, "毎月の基本生活費");
            ValidateNonNegative(errors, "IncomeExpense.Expenses.MonthlyRentManYen", expenses.MonthlyRentManYen, "毎月の家賃");
            ValidateNonNegative(errors, "IncomeExpense.Expenses.OtherAnnualCostManYen", expenses.OtherAnnualCostManYen, "その他支出");

            if (expenses.InflationRatePercent.HasValue &&
                !RateRules.IsInflationRateInRange(expenses.InflationRatePercent.Value))
            {
                errors.Add(new LifePlanValidationError(
                    "IncomeExpense.Expenses.InflationRatePercent",
                    LifePlanValidationMessages.InflationRateRange()));
            }
        }

        private static void ValidateRequiredAdultAge(
            List<LifePlanValidationError> errors,
            string key,
            int? age,
            string label)
        {
            if (!age.HasValue)
            {
                errors.Add(new LifePlanValidationError(key, LifePlanValidationMessages.Required(label)));
                return;
            }

            if (!AgeRules.IsAdultAgeInRange(age.Value))
            {
                errors.Add(new LifePlanValidationError(
                    key,
                    LifePlanValidationMessages.AdultAgeRange(label)));
            }
        }

        private static void ValidatePersonIncome(
            List<LifePlanValidationError> errors,
            string prefix,
            PersonIncomeInputViewModel income,
            string ownerLabel)
        {
            ValidateOccupation(errors, $"{prefix}.OccupationValue", income.OccupationValue, $"{ownerLabel}の職業");
            ValidateNonNegative(errors, $"{prefix}.AnnualIncomeManYen", income.AnnualIncomeManYen, $"{ownerLabel}の年収");
            ValidateAnnualIncomeChangeRate(errors, $"{prefix}.AnnualIncomeChangeRatePercent", income.AnnualIncomeChangeRatePercent, $"{ownerLabel}の年収の変化");
            ValidateNonNegative(errors, $"{prefix}.RetirementAllowanceManYen", income.RetirementAllowanceManYen, $"{ownerLabel}の退職金");
            ValidateNonNegative(errors, $"{prefix}.AnnualPensionManYen", income.AnnualPensionManYen, $"{ownerLabel}の年金年額");
            ValidateOptionalAdultAge(errors, $"{prefix}.WorkStartAge", income.WorkStartAge, $"{ownerLabel}の就労開始年齢");
            ValidateOptionalAdultAge(errors, $"{prefix}.WorkEndAge", income.WorkEndAge, $"{ownerLabel}の就労終了年齢");
            ValidateOptionalAdultAge(errors, $"{prefix}.PensionStartAge", income.PensionStartAge, $"{ownerLabel}の年金受取開始年齢");

            if (income.WorkStartAge.HasValue &&
                income.WorkEndAge.HasValue &&
                income.WorkEndAge.Value < income.WorkStartAge.Value)
            {
                errors.Add(new LifePlanValidationError(
                    $"{prefix}.WorkEndAge",
                    LifePlanValidationMessages.WorkEndAgeMustBeStartAgeOrLater(ownerLabel)));
            }

            if (income.WorkEndAge.HasValue &&
                income.PensionStartAge.HasValue &&
                income.PensionStartAge.Value <= income.WorkEndAge.Value)
            {
                errors.Add(new LifePlanValidationError(
                    $"{prefix}.PensionStartAge",
                    LifePlanValidationMessages.PensionStartAgeMustBeAfterWorkEndAge(ownerLabel)));
            }
        }

        private static void ValidateOccupation(
            List<LifePlanValidationError> errors,
            string key,
            string? value,
            string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (OccupationReferenceData.All.All(occupation => occupation.Value != value))
            {
                errors.Add(new LifePlanValidationError(key, LifePlanValidationMessages.DefinedOption(label)));
            }
        }

        private static void ValidateAnnualIncomeChangeRate(
            List<LifePlanValidationError> errors,
            string key,
            decimal? value,
            string label)
        {
            if (value.HasValue && !RateRules.IsAnnualIncomeChangeRateInRange(value.Value))
            {
                errors.Add(new LifePlanValidationError(
                    key,
                    LifePlanValidationMessages.AnnualIncomeChangeRateRange(label)));
            }
        }

        private static void ValidateOptionalAdultAge(
            List<LifePlanValidationError> errors,
            string key,
            int? age,
            string label)
        {
            if (age.HasValue && !AgeRules.IsAdultAgeInRange(age.Value))
            {
                errors.Add(new LifePlanValidationError(
                    key,
                    LifePlanValidationMessages.AdultAgeRange(label)));
            }
        }

        private static void ValidateNonNegative(
            List<LifePlanValidationError> errors,
            string key,
            decimal? value,
            string label)
        {
            if (value.HasValue && value.Value < 0)
            {
                errors.Add(new LifePlanValidationError(key, LifePlanValidationMessages.NonNegative(label)));
            }
        }

        private static void ValidatePositive(
            List<LifePlanValidationError> errors,
            string key,
            int? value,
            string label)
        {
            if (value.HasValue && value.Value < 1)
            {
                errors.Add(new LifePlanValidationError(key, LifePlanValidationMessages.Positive(label)));
            }
        }
    }
}
