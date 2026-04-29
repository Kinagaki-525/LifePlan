namespace rennsyu.Domain.ReferenceData
{
    public static class OccupationReferenceData
    {
        public static IReadOnlyList<OccupationEntry> All { get; } =
        [
            new("company_employee", "会社員"),
            new("self_employed", "自営業"),
            new("public_employee", "公務員"),
            new("homemaker", "専業主婦・主夫"),
            new("other", "その他")
        ];
    }
}
