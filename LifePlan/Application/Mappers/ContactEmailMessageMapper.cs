using LifePlan.Application.Dto;
using LifePlan.ViewModels.Contact;

namespace LifePlan.Application.Mappers
{
    public static class ContactEmailMessageMapper
    {
        private static readonly IReadOnlyDictionary<string, string> CategoryLabels = new Dictionary<string, string>
        {
            { "service", "サービスについて" },
            { "bug", "不具合報告" },
            { "lifeplan", "家計・ライフプラン相談" },
            { "request", "要望・改善アイデア" },
            { "partnership", "提携・広告について" },
            { "other", "その他" },
        };

        public static ContactEmailMessage ToEmailMessage(ContactViewModel input)
        {
            var categoryLabel = CategoryLabels.TryGetValue(input.Category ?? string.Empty, out var label)
                ? label
                : "（未選択）";

            return new ContactEmailMessage
            {
                Name = input.Name,
                Company = input.Company,
                Email = input.Email,
                CategoryLabel = categoryLabel,
                Subject = input.Subject ?? string.Empty,
                Message = input.Message ?? string.Empty
            };
        }
    }
}
