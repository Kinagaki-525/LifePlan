using rennsyu.Domain.Entities;
using rennsyu.ViewModels.LifePlan;

namespace rennsyu.Application.Results
{
    public class LifePlanSubmitResult
    {
        public LifePlanViewModel Page { get; set; } = new();

        public bool IsValid { get; set; }

        public IReadOnlyList<LifePlanValidationError> Errors { get; set; } = [];

        public LifePlanData? Data { get; set; }
    }
}
