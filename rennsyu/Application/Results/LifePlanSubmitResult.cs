using rennsyu.Domain.Logic;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Results
{
    public class LifePlanSubmitResult
    {
        public LifePlanViewModel Page { get; set; } = new();

        public bool IsValid { get; set; }

        public IReadOnlyList<LifePlanValidationError> Errors { get; set; } = [];

        public LifePlanCalculationResult? CalculationResult { get; set; }
    }
}
