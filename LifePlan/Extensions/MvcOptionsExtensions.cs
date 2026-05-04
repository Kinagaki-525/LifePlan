using LifePlan.Application.Validators;
using Microsoft.AspNetCore.Mvc;

namespace LifePlan.Extensions
{
    public static class MvcOptionsExtensions
    {
        public static void ConfigureLifePlanModelBindingMessages(this MvcOptions options)
        {
            var messages = options.ModelBindingMessageProvider;

            messages.SetAttemptedValueIsInvalidAccessor(LifePlanValidationMessages.AttemptedValueIsInvalid);
            messages.SetNonPropertyAttemptedValueIsInvalidAccessor(LifePlanValidationMessages.NonPropertyAttemptedValueIsInvalid);
            messages.SetUnknownValueIsInvalidAccessor(LifePlanValidationMessages.UnknownValueIsInvalid);
            messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => LifePlanValidationMessages.NonPropertyUnknownValueIsInvalid());
            messages.SetValueIsInvalidAccessor(LifePlanValidationMessages.ValueIsInvalid);
            messages.SetValueMustBeANumberAccessor(LifePlanValidationMessages.ValueMustBeANumber);
            messages.SetNonPropertyValueMustBeANumberAccessor(() => LifePlanValidationMessages.NonPropertyValueMustBeANumber());
            messages.SetValueMustNotBeNullAccessor(LifePlanValidationMessages.ValueMustNotBeNull);
            messages.SetMissingBindRequiredValueAccessor(LifePlanValidationMessages.MissingBindRequiredValue);
            messages.SetMissingKeyOrValueAccessor(() => LifePlanValidationMessages.MissingKeyOrValue());
        }
    }
}
