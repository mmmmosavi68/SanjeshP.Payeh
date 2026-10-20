using SanjeshP.Payeh.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.User
{
    public class UserVerificationCode : BaseEntity
    {
        [Required(ErrorMessage = "شناسه توکن الزامی است")]
        [Display(Name = "شناسه توکن")]
        [ForeignKey(nameof(UserToken))]
        public Guid TokenId { get; set; }

        [Required(ErrorMessage = "کد تایید الزامی است")]
        [MaxLength(10, ErrorMessage = "کد تایید نباید بیش از 10 کاراکتر باشد")]
        [Display(Name = "کد تایید")]
        public string VerificationCode { get; set; }

        [Required(ErrorMessage = "نوع تایید الزامی است")]
        [Display(Name = "نوع تایید")]
        public VerificationType VerificationType { get; set; }

        [MaxLength(200, ErrorMessage = "توضیحات کد تایید نباید بیش از 200 کاراکتر باشد")]
        [Display(Name = "توضیحات کد تایید")]
        public string VerificationCodeDesc { get; set; }

        [Required(ErrorMessage = "تاریخ انقضاء الزامی است")]
        [Display(Name = "تاریخ انقضاء")]
        public DateTime ExpirationDate { get; set; }

        [Display(Name = "کاربر توکن")]
        public virtual UserToken UserToken { get; set; }
    }

    public enum VerificationType
    {
        [Display(Name = "ایمیل")]
        Email = 1,

        [Display(Name = "پیامک")]
        SMS = 2,

        [Display(Name = "فراموشی رمز عبور")]
        PasswordReset = 3
    }
}
