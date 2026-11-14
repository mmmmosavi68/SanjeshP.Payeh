
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Users;

public sealed class BulkUserIdsDto
{
    [Required(ErrorMessage = "فهرست کاربران الزامی است")]
    [MinLength(1, ErrorMessage = "حداقل یک کاربر باید انتخاب شود")]
    [MaxLength(200, ErrorMessage = "حداکثر ۲۰۰ کاربر در هر درخواست قابل انتخاب است")]
    public List<Guid> UserIds { get; set; } = [];
}
