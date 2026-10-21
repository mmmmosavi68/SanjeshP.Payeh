using SanjeshP.Payeh.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.User
{
    public class UserToken : BaseEntity
    {
        //[Required(ErrorMessage = "شناسه کاربر الزامی است")]
        //[Display(Name = "شناسه کاربر")]
        //[ForeignKey("User")]
        //public Guid UserId { get; set; }

        //[Required(ErrorMessage = "شناسه جلسه الزامی است")]
        //[MaxLength(128, ErrorMessage = "شناسه جلسه نباید بیش از 128 کاراکتر باشد")]
        //[Display(Name = "شناسه جلسه")]
        //public string SessionId { get; set; }

        //[MaxLength(256, ErrorMessage = "Agent کاربر نباید بیش از 256 کاراکتر باشد")]
        //[Display(Name = "Agent کاربر")]
        //public string UserAgent { get; set; }

        //[Required(ErrorMessage = "تاریخ انقضاء الزامی است")]
        //[Display(Name = "تاریخ انقضاء")]
        //public DateTime ExpirationDate { get; set; }


        //[Display(Name = "کاربر")]
        //public virtual User User { get; set; }

        //public virtual ICollection<UserVerificationCode> UserVerificationCodes { get; set; } = new List<UserVerificationCode>();


        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public string Jti { get; set; } = null!;
        public string DeviceName { get; set; } = null!;
        public string IpAddress { get; set; } = null!;
        public DateTime TokenExpireDate { get; set; }
        public DateTime RefreshTokenExpireDate { get; set; }
        public DateTime? RevocationDate { get; set; }


    }
}
