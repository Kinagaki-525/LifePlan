using LifePlan.Domain.ReferenceData;
using LifePlan.ViewModels.LifePlan;

namespace LifePlan.Application.Factories
{
    public static class LifePlanRateSelectOptionFactory
    {
        public static IReadOnlyList<SelectOptionViewModel> CreateAnnualIncomeChangeRateOptions()
        {
            return CreateRateOptions(RateOptionCatalog.AnnualIncomeChangeRates);
        }

        public static IReadOnlyList<SelectOptionViewModel> CreateInflationRateOptions()
        {
            return CreateRateOptions(RateOptionCatalog.InflationRates);
        }

        private static IReadOnlyList<SelectOptionViewModel> CreateRateOptions(IReadOnlyList<RateOptionEntry> options)
        {
            return
            [
                new SelectOptionViewModel(string.Empty, "-（なし）"),
                .. options.Select(ToSelectOption)
            ];
        }

        private static SelectOptionViewModel ToSelectOption(RateOptionEntry option)
        {
            return new SelectOptionViewModel(option.RatePercent.ToString("0"), option.DisplayName);
        }
    }
}
