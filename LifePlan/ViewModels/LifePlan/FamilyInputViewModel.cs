namespace LifePlan.ViewModels.LifePlan
{
    public class FamilyInputViewModel
    {
        public int? HusbandAge { get; set; }

        public int? WifeAge { get; set; }

        public List<ChildInputViewModel> Children { get; set; } = [];
    }
}
