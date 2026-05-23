namespace LifePlan.Application.Options
{
    public sealed class SmtpSettings
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ToAddress { get; set; } = string.Empty;
    }
}
