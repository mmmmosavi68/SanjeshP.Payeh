namespace SanjeshP.Payeh.Api.DTOs.Groups
{
    public sealed class GroupMenuDto
    {
        public Guid Id { get; set; }
        public string MenuTitle { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public int? PageCode { get; set; }
        public string? Path { get; set; }
        public string? Icon { get; set; }
        public bool? ShowMenu { get; set; }
    }
}
