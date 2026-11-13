
using System.ComponentModel.DataAnnotations;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.WebFramework.Api;

namespace SanjeshP.Payeh.Api.DTOs.Menus;

public class UpdateMenuDto : BaseDto<UpdateMenuDto, Menu, Guid>
{
    [Required(ErrorMessage = "عنوان منو الزامی است")]
    [MaxLength(100, ErrorMessage = "عنوان منو نباید بیش از 100 کاراکتر باشد")]
    public string MenuTitle { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public int? PageCode { get; set; }

    [MaxLength(200)]
    public string? Path { get; set; }

    [MaxLength(100)]
    public string? version { get; set; }

    [MaxLength(100)]
    public string? ControllerName { get; set; }

    [MaxLength(100)]
    public string? ActionName { get; set; }

    public bool? ShowMenu { get; set; }

    [MaxLength(50)]
    public string? CssClass { get; set; }

    [MaxLength(100)]
    public string? Icon { get; set; }

    [MaxLength(200)]
    public string? Link { get; set; }

    public bool IsActive { get; set; }
}
