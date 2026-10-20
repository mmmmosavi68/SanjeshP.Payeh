using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SanjeshP.Payeh.Entities.Common
{
    public interface IEntity { }

    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; set; }
    }

    public abstract class BaseEntity<TKey> : IEntity<TKey>
    {
        [Key]
        public TKey Id { get; set; }


        [Required(ErrorMessage = "تاریخ ایجاد الزامی است")]
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "تاریخ به‌روزرسانی")]
        public DateTime? UpdatedDate { get; set; }

        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است")]
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "وضعیت حذف الزامی است")]
        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; } = false;

        [Required(ErrorMessage = "شناسه سازنده الزامی است")]
        [Display(Name = "شناسه سازنده")]
        public Guid CreatedBy { get; set; }

        [MaxLength(45, ErrorMessage = "آی‌پی میزبان نباید بیش از 45 کاراکتر باشد")]
        [Display(Name = "آی‌پی میزبان")]
        public string? HostIp { get; set; }

    }

    public abstract class BaseEntity : BaseEntity<Guid>
    {
        public BaseEntity()
        {
            Id = Guid.NewGuid();
        }
    }

    public class IntBaseEntity : BaseEntity<int> { }
}
