
using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Users;

public sealed class UpdateUserMenuAccessDto
{
    [MaxLength(500, ErrorMessage = "حداکثر ۵۰۰ منو قابل انتخاب است")]
    public List<Guid> MenuIds { get; set; } = [];
}
