using rennsyu.Application.Results;
using rennsyu.Domain.ReferenceData;
using rennsyu.Domain.Rules;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Validators
{
    public static class LifePlanInputValidator
    {
        public static IReadOnlyList<LifePlanValidationError> Validate(LifePlanViewModel page)
        {
            var errors = new List<LifePlanValidationError>();

            ValidateRequiredAdultAge(errors, "Family.HusbandAge", page.Family.HusbandAge, "夫の年齢");
            ValidateRequiredAdultAge(errors, "Family.WifeAge", page.Family.WifeAge, "妻の年齢");
            ValidateChildren(errors, page.Family.Children);
            ValidateSavings(errors, page.Savings);
            ValidatePersonIncome(errors, "IncomeExpense.HusbandIncome", page.IncomeExpense.HusbandIncome, "夫");
            ValidatePersonIncome(errors, "IncomeExpense.WifeIncome", page.IncomeExpense.WifeIncome, "妻");

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
                        $"子どもの年齢は{AgeRules.MinChildAge}〜{AgeRules.MaxChildAge}で入力してください。"));
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
                    $"想定運用年利は{RateRules.MinRatePercent}〜{RateRules.MaxExpectedAnnualReturnRatePercent}で入力してください。"));
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
                errors.Add(new LifePlanValidationError(key, $"{label}は必須です。"));
                return;
            }

            if (!AgeRules.IsAdultAgeInRange(age.Value))
            {
                errors.Add(new LifePlanValidationError(
                    key,
                    $"{label}は{AgeRules.MinAdultAge}〜{AgeRules.MaxAdultAge}で入力してください。"));
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
            ValidatePensionReference(errors, $"{prefix}.PensionReferenceValue", income.PensionReferenceValue, $"{ownerLabel}の年金参考値");

            ValidateOptionalAdultAge(errors, $"{prefix}.WorkStartAge", income.WorkStartAge, $"{ownerLabel}の就労開始年齢");
            ValidateOptionalAdultAge(errors, $"{prefix}.WorkEndAge", income.WorkEndAge, $"{ownerLabel}の就労終了年齢");
            ValidateOptionalAdultAge(errors, $"{prefix}.PensionStartAge", income.PensionStartAge, $"{ownerLabel}の年金受取開始年齢");

            if (income.WorkStartAge.HasValue &&
                income.WorkEndAge.HasValue &&
                income.WorkEndAge.Value < income.WorkStartAge.Value)
            {
                errors.Add(new LifePlanValidationError(
                    $"{prefix}.WorkEndAge",
                    $"{ownerLabel}の就労終了年齢は就労開始年齢以上にしてください。"));
            }

            if (income.WorkEndAge.HasValue &&
                income.PensionStartAge.HasValue &&
                income.PensionStartAge.Value <= income.WorkEndAge.Value)
            {
                errors.Add(new LifePlanValidationError(
                    $"{prefix}.PensionStartAge",
                    $"{ownerLabel}の年金受取開始年齢は就労終了年齢より後にしてください。"));
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
                errors.Add(new LifePlanValidationError(key, $"{label}は定義済みの選択肢から選んでください。"));
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
                    $"{label}は{RateRules.MinAnnualIncomeChangeRatePercent}〜{RateRules.MaxAnnualIncomeChangeRatePercent}%で入力してください。"));
            }
        }

        private static void ValidatePensionReference(
            List<LifePlanValidationError> errors,
            string key,
            string? value,
            string label)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (PensionReferenceData.All.All(reference => reference.Value != value))
            {
                errors.Add(new LifePlanValidationError(key, $"{label}は定義済みの選択肢から選んでください。"));
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
                    $"{label}は{AgeRules.MinAdultAge}〜{AgeRules.MaxAdultAge}で入力してください。"));
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
                errors.Add(new LifePlanValidationError(key, $"{label}は0以上で入力してください。"));
            }
        }
    }
}
