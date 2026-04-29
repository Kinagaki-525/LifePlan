namespace rennsyu.ViewModels.LifePlan
{
    public class LifeEventInputViewModel
    {
        public MarriageEventInputViewModel Marriage { get; set; } = new();

        public HousingEventInputViewModel Housing { get; set; } = new();

        public CarEventInputViewModel Car { get; set; } = new();

        public List<ChildEducationInputViewModel> EducationPlans { get; set; } = [];

        public TravelOtherEventInputViewModel TravelOther { get; set; } = new();
    }
}
