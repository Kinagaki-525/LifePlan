using rennsyu.Domain.Entities;

namespace rennsyu.Domain.Logic
{
    public class LifePlanCalculator
    {
        public LifePlanCalculationResult Calculate(LifePlanData input, int currentYear)
        {
            ArgumentNullException.ThrowIfNull(input);

            var yearsUntilHusbandTurns100 = 100 - input.Family.HusbandAge;
            var yearsUntilWifeTurns100 = 100 - input.Family.WifeAge;
            var simulationYears = Math.Max(yearsUntilHusbandTurns100, yearsUntilWifeTurns100);

            return new LifePlanCalculationResult(currentYear, currentYear + simulationYears);
        }
    }
}
