using System.ComponentModel.DataAnnotations;

namespace SanjeshP.Payeh.Api.DTOs.Groups;

public sealed class GroupMenuAccessDto
{
    public Guid MenuId { get; set; }

    public string MenuTitle { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public int? PageCode { get; set; }

    public string? Path { get; set; }

    public string? Version { get; set; }

    public string? ControllerName { get; set; }

    public string? ActionName { get; set; }

    public bool? ShowMenu { get; set; }

    public string? CssClass { get; set; }

    public string? Icon { get; set; }

    public string? Link { get; set; }

    public bool IsAssigned { get; set; }
}

public sealed class UpdateGroupMenuAccessDto
{
    [Required]
    public List<Guid> MenuIds { get; set; } = new();
}