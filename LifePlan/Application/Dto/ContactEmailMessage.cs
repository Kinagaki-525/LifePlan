namespace LifePlan.Application.Dto
{
    public sealed class ContactEmailMessage
    {
        public string Name { get; init; } = string.Empty;

        public string? Company { get; init; }

        public string Email { get; init; } = string.Empty;

        public string CategoryLabel { get; init; } = string.Empty;

        public string Subject { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;
    }
}
