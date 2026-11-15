
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Codes;

public sealed class UpdateCodeDto
{
    [Required(ErrorMessage = "عنوان الزامی است")]
    [MaxLength(200, ErrorMessage = "عنوان نباید بیشتر از ۲۰۰ کاراکتر باشد")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب والد الزامی است")]
    public int? ParentId { get; set; }

    public int? StandardCode { get; set; }

    public int? PrimaryCode { get; set; }

    public int? SubCode { get; set; }

    [MaxLength(400, ErrorMessage = "اطلاعات اضافی نباید بیشتر از ۴۰۰ کاراکتر باشد")]
    public string? AdditionalInformation { get; set; }

    public bool IsActive { get; set; } = true;
}
