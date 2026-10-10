namespace LifePlan.Domain.ReferenceData
{
    public static class ChildLivingCostMaster
    {
        public const int PriceBaseYear = 2024;

        public const string Source =
            "三澤・竹原「日本における0～18歳の子育てに要する費用の調査：ウェブアンケート調査2024」（日本公衆衛生雑誌73巻2号）表3の衣類・食費・生活用品費から算出";

        public static IReadOnlyList<ChildLivingCostEntry> Entries { get; } =
        [
            new(0, 2, 590_000, "2024年調査の0～2歳から算出"),
            new(3, 5, 590_000, "2024年調査の3～5歳から算出"),
            new(6, 11, 670_000, "2024年調査の小学1～6年から算出"),
            new(12, 14, 760_000, "2024年調査の中学1～3年から算出"),
            new(15, 17, 810_000, "2024年調査の高校1～3年から算出"),
            new(18, 23, 810_000, "高校生の額を継続する仮定")
        ];
    }
}
