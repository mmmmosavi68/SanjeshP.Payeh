using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Groups;

public sealed class BulkGroupUsersDto
{
    [Required(ErrorMessage = "فهرست کاربران الزامی است")]
    [MinLength(1, ErrorMessage = "حداقل یک کاربر باید انتخاب شود")]
    [MaxLength(200, ErrorMessage = "حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است")]
    public List<Guid> UserIds { get; set; } = [];
}