using LifePlan.Application.Validators;
using LifePlan.Domain.Rules;
using LifePlan.ViewModels.LifePlan;
using System.Globalization;

namespace LifePlan.Application.Factories
{
    public static class LifePlanClientValidationRuleFactory
    {
        public static LifePlanClientValidationViewModel Create()
        {
            return new LifePlanClientValidationViewModel
            {
                NumberMessage = LifePlanValidationMessages.NonPropertyValueMustBeANumber(),
                Rules = new Dictionary<string, LifePlanClientValidationRuleViewModel>
                {
                    ["Family.HusbandAge"] = RequiredAdultAge("夫の年齢"),
                    ["Family.WifeAge"] = RequiredAdultAge("妻の年齢"),
                    ["LifeEvents.Marriage.CostManYen"] = NonNegative("結婚費用"),
                    ["LifeEvents.Marriage.HusbandAge"] = OptionalAdultAge("結婚実施年齢"),
                    ["LifeEvents.Housing.PurchaseHusbandAge"] = OptionalAdultAge("住宅購入時期"),
                    ["LifeEvents.Housing.InterestRatePercent"] = HousingInterestRate(),
                    ["LifeEvents.Car.PurchaseAmountManYen"] = NonNegative("自動車購入額"),
                    ["LifeEvents.Car.FirstPurchaseHusbandAge"] = OptionalAdultAge("自動車の初回購入年齢"),
                    ["LifeEvents.TravelOther.AnnualCostManYen"] = NonNegative("旅行・その他の年間費用"),
                    ["LifeEvents.TravelOther.StartHusbandAge"] = OptionalAdultAge("旅行・その他の開始年齢"),
                    ["LifeEvents.TravelOther.EndHusbandAge"] = OptionalAdultAge("旅行・その他の終了年齢"),
                    ["IncomeExpense.HusbandIncome.AnnualIncomeManYen"] = NonNegative("夫の年収"),
                    ["IncomeExpense.HusbandIncome.WorkStartAge"] = OptionalAdultAge("夫の就労開始年齢"),
                    ["IncomeExpense.HusbandIncome.WorkEndAge"] = OptionalAdultAge("夫の就労終了年齢"),
                    ["IncomeExpense.HusbandIncome.RetirementAllowanceManYen"] = NonNegative("夫の退職金"),
                    ["IncomeExpense.HusbandIncome.AnnualPensionManYen"] = NonNegative("夫の年金年額"),
                    ["IncomeExpense.HusbandIncome.PensionStartAge"] = OptionalAdultAge("夫の年金受取開始年齢"),
                    ["IncomeExpense.WifeIncome.AnnualIncomeManYen"] = NonNegative("妻の年収"),
                    ["IncomeExpense.WifeIncome.WorkStartAge"] = OptionalAdultAge("妻の就労開始年齢"),
                    ["IncomeExpense.WifeIncome.WorkEndAge"] = OptionalAdultAge("妻の就労終了年齢"),
                    ["IncomeExpense.WifeIncome.RetirementAllowanceManYen"] = NonNegative("妻の退職金"),
                    ["IncomeExpense.WifeIncome.AnnualPensionManYen"] = NonNegative("妻の年金年額"),
                    ["IncomeExpense.WifeIncome.PensionStartAge"] = OptionalAdultAge("妻の年金受取開始年齢"),
                    ["Savings.CurrentFinancialAssetsManYen"] = NonNegative("現在の金融資産残高"),
                    ["Savings.ExpectedAnnualReturnRatePercent"] = RateRange(
                        "想定運用年利",
                        RateRules.MinRatePercent,
                        RateRules.MaxExpectedAnnualReturnRatePercent,
                        LifePlanValidationMessages.ExpectedAnnualReturnRateRange()),
                    ["IncomeExpense.Expenses.MonthlyBasicLivingCostManYen"] = NonNegative("毎月の基本生活費"),
                    ["IncomeExpense.Expenses.InflationRatePercent"] = RateRange(
                        "想定インフレ率",
                        RateRules.MinRatePercent,
                        RateRules.MaxInflationRatePercent,
                        LifePlanValidationMessages.InflationRateRange()),
                    ["IncomeExpense.Expenses.MonthlyRentManYen"] = NonNegative("毎月の家賃"),
                    ["IncomeExpense.Expenses.OtherAnnualCostManYen"] = NonNegative("その他支出")
                }
            };
        }

        private static LifePlanClientValidationRuleViewModel RequiredAdultAge(string label)
        {
            var rule = OptionalAdultAge(label);
            var attributes = rule.Attributes.ToDictionary(pair => pair.Key, pair => pair.Value);
            var messages = rule.Messages.ToDictionary(pair => pair.Key, pair => pair.Value);

            attributes["required"] = "required";
            attributes["maxlength"] = "3";
            attributes["data-rule-digits"] = "true";
            attributes["data-life-plan-half-width-integer"] = "true";
            messages["required"] = LifePlanValidationMessages.Required(label);
            messages["digits"] = LifePlanValidationMessages.HalfWidthInteger(label);

            return CreateRule(attributes, messages);
        }

        private static LifePlanClientValidationRuleViewModel OptionalAdultAge(string label)
        {
            return Range(
                label,
                AgeRules.MinAdultAge,
                AgeRules.MaxAdultAge,
                LifePlanValidationMessages.AdultAgeRange(label));
        }

        private static LifePlanClientValidationRuleViewModel NonNegative(string label)
        {
            return MinRate(label, LifePlanValidationMessages.NonNegative(label));
        }

        private static LifePlanClientValidationRuleViewModel HousingInterestRate()
        {
            return CreateRule(
                new Dictionary<string, string>
                {
                    ["min"] = RateRules.MinRatePercent.ToString("0", CultureInfo.InvariantCulture),
                    ["step"] = "0.1"
                },
                new Dictionary<string, string>
                {
                    ["min"] = LifePlanValidationMessages.HousingInterestRateRange(),
                    ["number"] = LifePlanValidationMessages.ValueMustBeANumber("想定金利"),
                    ["step"] = LifePlanValidationMessages.OneDecimalPlace("想定金利")
                });
        }

        private static LifePlanClientValidationRuleViewModel MinRate(string label, string message)
        {
            return CreateRule(
                new Dictionary<string, string> { ["min"] = RateRules.MinRatePercent.ToString("0", CultureInfo.InvariantCulture) },
                new Dictionary<string, string>
                {
                    ["min"] = message,
                    ["number"] = LifePlanValidationMessages.ValueMustBeANumber(label)
                });
        }

        private static LifePlanClientValidationRuleViewModel Range(
            string label,
            int min,
            int max,
            string message)
        {
            return CreateRule(
                new Dictionary<string, string>
                {
                    ["min"] = min.ToString(CultureInfo.InvariantCulture),
                    ["max"] = max.ToString(CultureInfo.InvariantCulture)
                },
                RangeMessages(label, message));
        }

        private static LifePlanClientValidationRuleViewModel RateRange(
            string label,
            decimal min,
            decimal max,
            string message)
        {
            return CreateRule(
                new Dictionary<string, string>
                {
                    ["min"] = min.ToString("0.##", CultureInfo.InvariantCulture),
                    ["max"] = max.ToString("0.##", CultureInfo.InvariantCulture)
                },
                RangeMessages(label, message));
        }

        private static Dictionary<string, string> RangeMessages(string label, string message)
        {
            return new Dictionary<string, string>
            {
                ["min"] = message,
                ["max"] = message,
                ["number"] = LifePlanValidationMessages.ValueMustBeANumber(label)
            };
        }

        private static LifePlanClientValidationRuleViewModel CreateRule(
            IReadOnlyDictionary<string, string> attributes,
            IReadOnlyDictionary<string, string> messages)
        {
            return new LifePlanClientValidationRuleViewModel
            {
                Attributes = attributes,
                Messages = messages
            };
        }
    }
}
