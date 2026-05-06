namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanClientValidationViewModel
    {
        public string NumberMessage { get; set; } = string.Empty;

        public IReadOnlyDictionary<string, LifePlanClientValidationRuleViewModel> Rules { get; set; } =
            new Dictionary<string, LifePlanClientValidationRuleViewModel>();
    }
}
