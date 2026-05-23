using System.Net;
using System.Net.Mail;
using LifePlan.Application.Dto;
using LifePlan.Application.Interfaces;
using LifePlan.Application.Options;
using Microsoft.Extensions.Options;

namespace LifePlan.Infrastructure.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings settings;
        private readonly ILogger<SmtpEmailSender> logger;

        public SmtpEmailSender(IOptions<SmtpSettings> options, ILogger<SmtpEmailSender> logger)
        {
            settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendContactAsync(ContactEmailMessage message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            ValidateSettings();

            var adminBody = $@"お問い合わせが届きました。

【お名前】         {message.Name}
【会社名】         {message.Company ?? "（未入力）"}
【メールアドレス】 {message.Email}
【種別】           {message.CategoryLabel}
【件名】           {message.Subject}

【内容】
{message.Message}
";

            var replyBody = $@"{message.Name} 様

お問い合わせいただき、ありがとうございます。
通常1〜2営業日以内にご返信いたします。

このメールは自動送信です。返信はお控えください。

───────────────
ふたりの家計
───────────────
";

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(settings.User, settings.Password),
                EnableSsl = true,
            };

            using var toAdmin = new MailMessage
            {
                From = new MailAddress(settings.User, "ふたりの家計 お問い合わせ"),
                Subject = $"【お問い合わせ】{message.Subject}",
                Body = adminBody,
                IsBodyHtml = false,
            };
            toAdmin.To.Add(settings.ToAddress);
            toAdmin.ReplyToList.Add(new MailAddress(message.Email, message.Name));

            await client.SendMailAsync(toAdmin);

            using var toUser = new MailMessage
            {
                From = new MailAddress(settings.User, "ふたりの家計"),
                Subject = "【ふたりの家計】お問い合わせを受け付けました",
                Body = replyBody,
                IsBodyHtml = false,
            };
            toUser.To.Add(new MailAddress(message.Email, message.Name));

            try
            {
                await client.SendMailAsync(toUser);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "送信者への自動返信メールの送信に失敗しました。管理者への通知は完了しました。");
            }
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(settings.Host))
            {
                throw new InvalidOperationException("SMTP 設定が不足しています: Smtp:Host を設定してください。");
            }

            if (string.IsNullOrWhiteSpace(settings.User))
            {
                throw new InvalidOperationException("SMTP 設定が不足しています: Smtp:User を設定してください。");
            }

            if (string.IsNullOrWhiteSpace(settings.Password))
            {
                throw new InvalidOperationException("SMTP 設定が不足しています: Smtp:Password を設定してください。");
            }

            if (string.IsNullOrWhiteSpace(settings.ToAddress))
            {
                throw new InvalidOperationException("SMTP 設定が不足しています: Smtp:ToAddress を設定してください。");
            }
        }
    }
}
