
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.DTOs.Menus;

public class MenuDto : BaseDto<MenuDto, Menu, Guid>
{
    public string MenuTitle { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public int? PageCode { get; set; }

    public string? Path { get; set; }

    public string? version { get; set; }

    public string? ControllerName { get; set; }

    public string? ActionName { get; set; }

    public bool? ShowMenu { get; set; }

    public string? CssClass { get; set; }

    public string? Icon { get; set; }

    public string? Link { get; set; }

    public bool IsActive { get; set; }
}
