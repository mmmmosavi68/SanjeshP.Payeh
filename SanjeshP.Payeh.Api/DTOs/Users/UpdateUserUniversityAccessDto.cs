using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Users;

public sealed class UpdateUserUniversityAccessDto
{
    [MaxLength(500, ErrorMessage = "حداکثر ۵۰۰ دانشگاه قابل انتخاب است")]
    public List<int> UniversityIds { get; set; } = [];
}