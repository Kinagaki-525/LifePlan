namespace rennsyu.Domain.ReferenceData
{
    public static class EducationCostMaster
    {
        public static IReadOnlyList<EducationCostEntry> Entries { get; } =
        [
            new("nursery_public", "保育園", "公立", 0, 2, 45, 45),
            new("nursery_private", "保育園", "私立", 0, 2, 55, 55),
            new("kindergarten_public", "幼稚園・保育園", "公立", 3, 5, 20, 20),
            new("kindergarten_private", "幼稚園・保育園", "私立", 3, 5, 35, 35),
            new("elementary_public", "小学校", "公立", 6, 11, 40, 40),
            new("elementary_private", "小学校", "私立", 6, 11, 180, 180),
            new("junior_high_public", "中学校", "公立", 12, 14, 55, 55),
            new("junior_high_private", "中学校", "私立", 12, 14, 160, 160),
            new("high_public", "高校", "公立", 15, 17, 60, 60),
            new("high_private", "高校", "私立", 15, 17, 105, 105),
            new("university_public", "大学", "国公立", 18, 21, 85, 55),
            new("university_private_liberal_arts", "大学", "私立文系", 18, 21, 120, 100),
            new("university_private_science", "大学", "私立理系", 18, 21, 155, 130),
            new("graduate_public", "大学院", "国公立", 22, 23, 85, 55),
            new("graduate_private", "大学院", "私立", 22, 23, 120, 100)
        ];

    }
}
