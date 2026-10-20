using SanjeshP.Payeh.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.User
{
    public class UserProfile : BaseEntity<int>
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        [Display(Name = "شناسه کاربر")]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        [MaxLength(50, ErrorMessage = "نام نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(50, ErrorMessage = "نام خانوادگی نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [MaxLength(50, ErrorMessage = "نام پدر نباید بیش از 50 کاراکتر باشد")]
        [Display(Name = "نام پدر")]
        public string FatherName { get; set; }

        [Required(ErrorMessage = "کد ملی الزامی است")]
        [MaxLength(10, ErrorMessage = "کد ملی نباید بیش از 10 کاراکتر باشد")]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "جنسیت")]
        public GenderType? Gender { get; set; }

        [Display(Name = "وضعیت تاهل")]
        public MaritalStatusType? MaritalStatus { get; set; }

        [Display(Name = "دین")]
        public int? Religion { get; set; }

        [Display(Name = "چپ دست")]
        public LeftHandedType? LeftHanded { get; set; }

        [Display(Name = "ملیت")]
        public int? Nationality { get; set; }

        [Display(Name = "کاربر")]
        public virtual User User { get; set; }
    }

    public enum GenderType
    {
        [Display(Name = "مرد")]
        Male = 10,

        [Display(Name = "زن")]
        Female = 11
    }

    public enum MaritalStatusType
    {
        [Display(Name = "مجرد")]
        Single = 10,

        [Display(Name = "متأهل")]
        Married = 11
    }

    public enum LeftHandedType
    {
        [Display(Name = "راست دست")]
        Right = 0,

        [Display(Name = "چپ دست")]
        Left = 11
    }
}
