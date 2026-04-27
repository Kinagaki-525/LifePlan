namespace rennsyu.Domain.Entities
{
    public class FamilyData
    {
        public int HusbandAge { get; set; }

        public int WifeAge { get; set; }

        public List<ChildData> Children { get; set; } = [];
    }
}
