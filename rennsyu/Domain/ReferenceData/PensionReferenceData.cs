namespace rennsyu.Domain.ReferenceData
{
    public static class PensionReferenceData
    {
        public static IReadOnlyList<PensionReferenceEntry> All { get; } =
        [
            new("kousei_male", "厚生年金＋国民年金（男性）", 2_043_180, 170_265),
            new("kousei_female", "厚生年金＋国民年金（女性）", 1_247_868, 103_739),
            new("kokumin_male", "国民年金のみ（男性）", 654_180, 54_515),
            new("kokumin_female", "国民年金のみ（女性）", 577_140, 48_095)
        ];
    }
}
