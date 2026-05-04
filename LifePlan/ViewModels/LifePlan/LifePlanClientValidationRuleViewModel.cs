namespace LifePlan.ViewModels.LifePlan
{
    public class LifePlanClientValidationRuleViewModel
    {
        public IReadOnlyDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> Messages { get; set; } = new Dictionary<string, string>();
    }
}
