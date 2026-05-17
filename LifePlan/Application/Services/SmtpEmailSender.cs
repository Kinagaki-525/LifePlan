using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using LifePlan.Application.Interfaces;
using LifePlan.Models;

namespace LifePlan.Application.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        private static readonly IReadOnlyDictionary<string, string> CategoryLabels = new Dictionary<string, string>
        {
            { "service",     "サービスについて" },
            { "bug",         "不具合報告" },
            { "lifeplan",    "家計・ライフプラン相談" },
            { "request",     "要望・改善アイデア" },
            { "partnership", "提携・広告について" },
            { "other",       "その他" },
        };

        public SmtpEmailSender(IOptions<SmtpSettings> options, ILogger<SmtpEmailSender> logger)
        {
            _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ValidateSettings();
        }

        public async Task SendContactAsync(ContactViewModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var categoryLabel = CategoryLabels.TryGetValue(model.Category ?? string.Empty, out var label)
                ? label
                : "（未選択）";

            var adminBody = $@"お問い合わせが届きました。

【お名前】         {model.Name}
【会社名】         {model.Company ?? "（未入力）"}
【メールアドレス】 {model.Email}
【種別】           {categoryLabel}
【件名】           {model.Subject ?? "（未入力）"}

【内容】
{model.Message ?? "（未入力）"}
";

            var replyBody = $@"{model.Name} 様

お問い合わせいただき、ありがとうございます。
通常1〜2営業日以内にご返信いたします。

このメールは自動送信です。返信はお控えください。

───────────────
ふたりの家計
───────────────
";

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.User, _settings.Password),
                EnableSsl = true,
            };

            var toAdmin = new MailMessage
            {
                From = new MailAddress(_settings.User, "ふたりの家計 お問い合わせ"),
                Subject = $"【お問い合わせ】{model.Subject ?? categoryLabel}",
                Body = adminBody,
                IsBodyHtml = false,
            };
            toAdmin.To.Add(_settings.ToAddress);
            toAdmin.ReplyToList.Add(new MailAddress(model.Email, model.Name));

            await client.SendMailAsync(toAdmin);

            var toUser = new MailMessage
            {
                From = new MailAddress(_settings.ToAddress, "ふたりの家計"),
                Subject = "【ふたりの家計】お問い合わせを受け付けました",
                Body = replyBody,
                IsBodyHtml = false,
            };
            toUser.To.Add(new MailAddress(model.Email, model.Name));

            try
            {
                await client.SendMailAsync(toUser);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "送信者への自動返信メールの送信に失敗しました。管理者への通知は完了しました。");
            }
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.Host))
            {
                throw new InvalidOperationException("SMTP 設定が不足しています: Smtp:Host を設定してください。");
            }

            if (string.IsNullOrWhiteSpace(_settings.User))
            {
                throw new InvalidOperationException("SMTP 設定が不足しています: Smtp:User を設定してください。");
            }

            if (string.IsNullOrWhiteSpace(_settings.Password))
            {
                throw new InvalidOperationException("SMTP 設定が不足しています: Smtp:Password を設定してください。");
            }

            if (string.IsNullOrWhiteSpace(_settings.ToAddress))
            {
                _settings.ToAddress = "futari.kakei@gmail.com";
                _logger.LogWarning("Smtp:ToAddress が設定されていません。デフォルトの宛先を使用します。");
            }
        }
    }
}
