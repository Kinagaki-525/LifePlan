using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LifePlan.Models
{
    public sealed class RequiredTrueAttribute : ValidationAttribute, IClientModelValidator
    {
        public RequiredTrueAttribute()
        {
        }

        public override bool IsValid(object? value)
        {
            return value is bool boolean && boolean;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-requiredtrue", ErrorMessage ?? "この項目は必須です。");
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }

            attributes.Add(key, value);
            return true;
        }
    }
}
