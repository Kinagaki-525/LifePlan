namespace LifePlan.Domain.Entities
{
    public class LifeEventData
    {
        public MarriageEventData Marriage { get; set; } = new();

        public HousingEventData Housing { get; set; } = new();

        public CarEventData Car { get; set; } = new();

        public List<ChildEducationData> EducationPlans { get; set; } = [];

        public TravelOtherEventData TravelOther { get; set; } = new();
    }
}
