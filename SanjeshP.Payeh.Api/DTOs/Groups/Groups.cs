using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Groups;

public sealed class GroupDto
{
    public Guid Id { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int UserCount { get; set; }
}

public sealed class CreateGroupDto
{
    [Required(ErrorMessage = "عنوان گروه الزامی است")]
    [MaxLength(100, ErrorMessage = "عنوان گروه نباید بیش از 100 کاراکتر باشد")]
    public string GroupName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateGroupDto
{
    [Required(ErrorMessage = "عنوان گروه الزامی است")]
    [MaxLength(100, ErrorMessage = "عنوان گروه نباید بیش از 100 کاراکتر باشد")]
    public string GroupName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public sealed class AddGroupUserDto
{
    [Required]
    public Guid UserId { get; set; }
}

public sealed class GroupMemberDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }
}