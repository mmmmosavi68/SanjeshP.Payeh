
namespace SanjeshP.Payeh.Api.DTOs.Codes;

public sealed class CodeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int? ParentPrimaryCode { get; set; }
    public int? ParentSubCode { get; set; }
    public int? StandardCode { get; set; }
    public int? PrimaryCode { get; set; }
    public int? SubCode { get; set; }
    public string? AdditionalInformation { get; set; }
    public bool IsActive { get; set; }
}
