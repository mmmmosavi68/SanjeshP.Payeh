namespace SanjeshP.Payeh.Api.Models.Auth;

/// <summary>یک گره از منوی مجاز کاربر؛ شامل مجوزهای سطح Action.</summary>
public sealed class MenuPermissionDto
{
    public Guid Id { get; init; }
    public string MenuTitle { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public int? PageCode { get; init; }
    public string? Version { get; init; }
    public string? ControllerName { get; init; }
    public string? ActionName { get; init; }
    public bool ShowMenu { get; init; }
    public string? Icon { get; init; }
    public string? Link { get; init; }
}
