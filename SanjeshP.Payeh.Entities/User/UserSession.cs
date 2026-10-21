using SanjeshP.Payeh.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.User
{
    /// <summary>
    /// نشست ورود کاربر (تغییر نام یافته از UserToken).
    /// هر ورود (هر دستگاه) یک رکورد مستقل است؛ خروج از یک دستگاه فقط همین رکورد را باطل می‌کند.
    /// Refresh Token به‌صورت Hash ذخیره می‌شود و در هر Refresh چرخش (Rotation) دارد.
    /// </summary>
    public class UserSession : BaseEntity
    {
        /// <summary>شناسه‌ی کاربر مالک نشست.</summary>
        [Display(Name = "شناسه کاربر")]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        /// <summary>شناسه‌ی یکتای آخرین Access Token صادرشده برای این نشست (jti).</summary>
        [Required]
        [MaxLength(64)]
        [Display(Name = "شناسه توکن")]
        public string Jti { get; set; } = null!;

        /// <summary>هش SHA-256 توکن تازه‌سازی فعلی. خود توکن هرگز ذخیره نمی‌شود.</summary>
        [Required]
        [MaxLength(128)]
        [Display(Name = "هش توکن تازه‌سازی")]
        public string RefreshTokenHash { get; set; } = null!;

        /// <summary>
        /// هش توکن تازه‌سازی قبلی؛ برای تشخیص استفاده‌ی مجدد از توکن چرخش‌یافته (سرقت توکن).
        /// اگر توکنی با این هش برسد، کل نشست باطل می‌شود.
        /// </summary>
        [MaxLength(128)]
        [Display(Name = "هش توکن تازه‌سازی قبلی")]
        public string? PreviousRefreshTokenHash { get; set; }

        /// <summary>نام دستگاه اعلام‌شده توسط کلاینت (مثلاً «کروم - ویندوز»).</summary>
        [MaxLength(100)]
        [Display(Name = "نام دستگاه")]
        public string DeviceName { get; set; } = null!;

        /// <summary>User-Agent درخواست ورود.</summary>
        [MaxLength(512)]
        [Display(Name = "عامل کاربر")]
        public string? UserAgent { get; set; }

        /// <summary>آی‌پی زمان ورود.</summary>
        [MaxLength(45)]
        [Display(Name = "آی‌پی")]
        public string IpAddress { get; set; } = null!;

        /// <summary>تاریخ انقضای آخرین Access Token این نشست.</summary>
        [Display(Name = "انقضای توکن دسترسی")]
        public DateTime TokenExpireDate { get; set; }

        /// <summary>تاریخ انقضای Refresh Token فعلی.</summary>
        [Display(Name = "انقضای توکن تازه‌سازی")]
        public DateTime RefreshTokenExpireDate { get; set; }

        /// <summary>آخرین باری که نشست استفاده شده (ورود یا Refresh).</summary>
        [Display(Name = "آخرین استفاده")]
        public DateTime? LastUsedAt { get; set; }

        /// <summary>تاریخ ابطال نشست؛ null یعنی نشست فعال است.</summary>
        [Display(Name = "تاریخ ابطال")]
        public DateTime? RevocationDate { get; set; }

        /// <summary>کاربر مالک نشست.</summary>
        [Display(Name = "کاربر")]
        public virtual User User { get; set; } = null!;

        /// <summary>آیا نشست در حال حاضر فعال است؟</summary>
        [NotMapped]
        public bool IsCurrentlyActive =>
            RevocationDate is null && RefreshTokenExpireDate > DateTime.UtcNow && !IsDeleted;
    }
}