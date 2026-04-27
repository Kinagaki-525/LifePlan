using rennsyu.Domain.ReferenceData;
using rennsyu.Domain.Rules;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Mappers
{
    public static class LifePlanPageMapper
    {
        public static List<ChildInputViewModel> CreateChildInputs()
        {
            return CreateChildLabels()
                .Select(label => new ChildInputViewModel { Label = label })
                .ToList();
        }

        public static List<ChildEducationInputViewModel> CreateEducationPlans()
        {
            return CreateChildLabels()
                .Select(label => new ChildEducationInputViewModel { Label = label })
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> CreateChildAgeOptions()
        {
            var options = new List<SelectOptionViewModel>
            {
                new(string.Empty, "-（なし）")
            };

            for (var age = AgeRules.MinChildAge; age <= AgeRules.MaxChildAge; age++)
            {
                options.Add(new SelectOptionViewModel(age.ToString(), age.ToString()));
            }

            return options;
        }

        public static IReadOnlyList<SelectOptionViewModel> ToEducationOptions(IReadOnlyList<EducationCostEntry> entries)
        {
            return entries
                .Select(entry => new SelectOptionViewModel(entry.Value, $"{entry.Stage}：{entry.Type}"))
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> ToPensionReferenceOptions(IReadOnlyList<PensionReferenceEntry> references)
        {
            return references
                .Select(reference => new SelectOptionViewModel(reference.Value, reference.DisplayName))
                .ToList();
        }

        public static IReadOnlyList<SelectOptionViewModel> CreateOccupationOptions()
        {
            return
            [
                new SelectOptionViewModel("company_employee", "会社員"),
                new SelectOptionViewModel("self_employed", "自営業"),
                new SelectOptionViewModel("public_employee", "公務員"),
                new SelectOptionViewModel("homemaker", "専業主婦・主夫"),
                new SelectOptionViewModel("other", "その他")
            ];
        }

        private static IReadOnlyList<string> CreateChildLabels()
        {
            return
            [
                "第1子",
                "第2子",
                "第3子",
                "第4子"
            ];
        }
    }
}
