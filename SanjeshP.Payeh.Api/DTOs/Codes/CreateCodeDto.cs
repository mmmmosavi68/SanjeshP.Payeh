
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Codes;

public sealed class CreateCodeDto
{
    [Required(ErrorMessage = "عنوان الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    /// <summary>شناسه والد (سازگاری قبلی). به‌جای آن می‌توان ParentPrimaryCode و ParentSubCode را فرستاد.</summary>
    public int? ParentId { get; set; }

    public int? ParentPrimaryCode { get; set; }

    public int? ParentSubCode { get; set; }

    public int? StandardCode { get; set; }

    public int? PrimaryCode { get; set; }

    public int? SubCode { get; set; }

    [MaxLength(400, ErrorMessage = "اطلاعات اضافی نباید بیشتر از ۴۰۰ کاراکتر باشد")]
    public string? AdditionalInformation { get; set; }
}
