namespace LifePlan.Domain.Rules
{
    public static class AgeRules
    {
        public const int MinAdultAge = 16;
        public const int MaxAdultAge = 100;

        public const int MinChildAge = -20;
        public const int MaxChildAge = 30;

        public static bool IsAdultAgeInRange(int age)
        {
            return age is >= MinAdultAge and <= MaxAdultAge;
        }

        public static bool IsChildAgeInRange(int age)
        {
            return age is >= MinChildAge and <= MaxChildAge;
        }
    }
}
