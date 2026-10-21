using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.User
{
    /// <summary>
    /// کد یک‌بارمصرف پیامکی.
    /// تغییر مهم امنیتی: کد به‌صورت متن ساده ذخیره نمی‌شود؛ فقط هش SHA-256 آن
    /// (به‌همراه نمکِ شناسه‌ی رکورد) در <see cref="CodeHash"/> نگهداری می‌شود.
    /// </summary>
    public class UserOtp : BaseEntity
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        [Display(Name = "شناسه کاربر")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        /// <summary>
        /// هش کد به شکل SHA256("{Id}:{Code}"). شناسه‌ی رکورد نقش نمک را دارد
        /// تا کدهای یکسان در رکوردهای مختلف، هش یکسان تولید نکنند.
        /// (جایگزین ستون Code که متن ساده بود — تغییر Breaking در سطح دیتابیس)
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Display(Name = "هش کد یک‌بارمصرف")]
        public string CodeHash { get; set; } = null!;

        [Required(ErrorMessage = "تاریخ انقضا الزامی است")]
        [Display(Name = "تاریخ انقضا")]
        public DateTime ExpireAt { get; set; }

        /// <summary>تعداد تلاش‌های ناموفق برای این کد؛ پس از عبور از حد مجاز، کد قفل می‌شود.</summary>
        [Display(Name = "تعداد تلاش ناموفق")]
        public int FailedAttempts { get; set; }

        [Display(Name = "مصرف شده")]
        public bool IsUsed { get; set; }

        /// <summary>زمان مصرف موفق کد (برای گزارش و حسابرسی).</summary>
        [Display(Name = "زمان مصرف")]
        public DateTime? ConsumedAt { get; set; }

        [MaxLength(100)]
        [Display(Name = "مقصد ارسال")]
        public string? SentTo { get; set; }

        /// <summary>هدف کد؛ مثل Login. امکان استفاده‌ی مجدد از همین جدول برای سناریوهای دیگر.</summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "هدف کد")]
        public string Purpose { get; set; } = "Login";

        [Display(Name = "کاربر")]
        public virtual User User { get; set; } = null!;
    }
}