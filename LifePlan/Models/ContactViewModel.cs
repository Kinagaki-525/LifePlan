using System.ComponentModel.DataAnnotations;

namespace LifePlan.Models  // ← プロジェクト名に変更
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "お名前を入力してください")]
        public string Name { get; set; } = string.Empty;

        public string? Company { get; set; }

        [Required(ErrorMessage = "メールアドレスを入力してください")]
        [EmailAddress(ErrorMessage = "正しいメールアドレスを入力してください")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "お問い合わせ種別を選択してください")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "件名を入力してください")]
        public string? Subject { get; set; }

        [Required(ErrorMessage = "お問い合わせ内容を入力してください")]
        public string? Message { get; set; }

        [RequiredTrue(ErrorMessage = "プライバシーポリシーへの同意が必要です")]
        public bool AgreePrivacy { get; set; }
    }
}