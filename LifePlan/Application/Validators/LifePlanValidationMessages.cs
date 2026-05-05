using LifePlan.Domain.Rules;

namespace LifePlan.Application.Validators
{
    public static class LifePlanValidationMessages
    {
        public static string Required(string label)
        {
            return $"{label}は必須です。";
        }

        public static string AdultAgeRange(string label)
        {
            return $"{label}は{AgeRules.MinAdultAge}〜{AgeRules.MaxAdultAge}で入力してください。";
        }

        public static string HalfWidthInteger(string label)
        {
            return $"{label}は半角整数で入力してください。";
        }

        public static string HalfWidthInteger(string label)
        {
            return $"{label}は半角整数で入力してください。";
        }

        public static string ChildAgeRange()
        {
            return $"子どもの年齢は{AgeRules.MinChildAge}〜{AgeRules.MaxChildAge}で入力してください。";
        }

        public static string NonNegative(string label)
        {
            return $"{label}は0以上で入力してください。";
        }

        public static string Positive(string label)
        {
            return $"{label}は1以上で入力してください。";
        }

        public static string ExpectedAnnualReturnRateRange()
        {
            return $"想定運用年利は{RateRules.MinRatePercent}〜{RateRules.MaxExpectedAnnualReturnRatePercent}で入力してください。";
        }

        public static string HousingInterestRateRange()
        {
            return $"住宅ローンの想定金利は{RateRules.MinRatePercent}%以上で入力してください。";
        }

        public static string OneDecimalPlace(string label)
        {
            return $"{label}は小数第1位までで入力してください。";
        }

        public static string OneDecimalPlace(string label)
        {
            return $"{label}は小数第1位までで入力してください。";
        }

        public static string DefinedOption(string label)
        {
            return $"{label}は定義済みの選択肢から選んでください。";
        }

        public static string EducationDefinedOption(string stage)
        {
            return $"{stage}の教育費は定義済みの選択肢から選んでください。";
        }

        public static string EndAgeMustBeStartAgeOrLater(string label)
        {
            return $"{label}は開始年齢以上にしてください。";
        }

        public static string WorkEndAgeMustBeStartAgeOrLater(string ownerLabel)
        {
            return $"{ownerLabel}の就労終了年齢は就労開始年齢以上にしてください。";
        }

        public static string PensionStartAgeMustBeAfterWorkEndAge(string ownerLabel)
        {
            return $"{ownerLabel}の年金受取開始年齢は就労終了年齢より後にしてください。";
        }

        public static string AttemptedValueIsInvalid(string value, string fieldName)
        {
            return $"{fieldName}に入力された値「{value}」は正しくありません。";
        }

        public static string NonPropertyAttemptedValueIsInvalid(string value)
        {
            return $"入力された値「{value}」は正しくありません。";
        }

        public static string UnknownValueIsInvalid(string fieldName)
        {
            return $"{fieldName}の値は正しくありません。";
        }

        public static string NonPropertyUnknownValueIsInvalid()
        {
            return "入力された値は正しくありません。";
        }

        public static string ValueIsInvalid(string value)
        {
            return $"入力された値「{value}」は正しくありません。";
        }

        public static string ValueMustBeANumber(string fieldName)
        {
            return $"{fieldName}は数値で入力してください。";
        }

        public static string NonPropertyValueMustBeANumber()
        {
            return "数値で入力してください。";
        }

        public static string ValueMustNotBeNull(string fieldName)
        {
            return $"{fieldName}は必須です。";
        }

        public static string MissingBindRequiredValue(string fieldName)
        {
            return $"{fieldName}は必須です。";
        }

        public static string MissingKeyOrValue()
        {
            return "キーまたは値が不足しています。";
        }
    }
}
