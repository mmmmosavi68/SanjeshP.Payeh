using Riok.Mapperly.Abstractions;
using SanjeshP.Payeh.Api.DTOs.Codes;
using SanjeshP.Payeh.Api.DTOs.Exams;
using SanjeshP.Payeh.Api.DTOs.ExamSessions;
using SanjeshP.Payeh.Api.DTOs.Menus;
using SanjeshP.Payeh.Api.DTOs.Universities;
using SanjeshP.Payeh.Api.Models.Users;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.University;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Api.Mapping;

/// <summary>
/// پیاده‌سازی Source Generator. هر عضو بدون نگاشت در مقصد (Entity→DTO) خطای Build است؛
/// برای DTO→Entity هر ویژگی DTO که نگاشت نشود خطای Build می‌دهد.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, ThrowOnMappingNullMismatch = false)]
public sealed partial class ApiMapper : IApiMapper
{
    // ───── Codes ─────
    public partial CodeDto ToDto(Code entity);
    public partial IQueryable<CodeDto> ProjectToDto(IQueryable<Code> query);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(CreateCodeDto.ParentPrimaryCode))]
    [MapperIgnoreSource(nameof(CreateCodeDto.ParentSubCode))]
    public partial Code ToEntity(CreateCodeDto dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(UpdateCodeDto.ParentPrimaryCode))]
    [MapperIgnoreSource(nameof(UpdateCodeDto.ParentSubCode))]
    public partial Code ToEntity(UpdateCodeDto dto);

    // ───── Universities ─────
    public partial UniversityDto ToDto(University entity);
    public partial IQueryable<UniversityDto> ProjectToDto(IQueryable<University> query);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(CreateUniversityDto.UniversityTypePrimaryCode))]
    [MapperIgnoreSource(nameof(CreateUniversityDto.UniversityTypeSubCode))]
    public partial University ToEntity(CreateUniversityDto dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(UpdateUniversityDto.UniversityTypePrimaryCode))]
    [MapperIgnoreSource(nameof(UpdateUniversityDto.UniversityTypeSubCode))]
    public partial University ToEntity(UpdateUniversityDto dto);

    // ───── Exams ─────
    [MapProperty("ExamSession.ExamDate", nameof(ExamDto.ExamDate))]
    public partial ExamDto ToDto(Exam entity);

    public partial IQueryable<ExamDto> ProjectToDto(IQueryable<Exam> query);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(CreateExamDto.ExamTypePrimaryCode))]
    [MapperIgnoreSource(nameof(CreateExamDto.ExamTypeSubCode))]
    [MapperIgnoreSource(nameof(CreateExamDto.ExamModePrimaryCode))]
    [MapperIgnoreSource(nameof(CreateExamDto.ExamModeSubCode))]
    public partial Exam ToEntity(CreateExamDto dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(UpdateExamDto.ExamTypePrimaryCode))]
    [MapperIgnoreSource(nameof(UpdateExamDto.ExamTypeSubCode))]
    [MapperIgnoreSource(nameof(UpdateExamDto.ExamModePrimaryCode))]
    [MapperIgnoreSource(nameof(UpdateExamDto.ExamModeSubCode))]
    public partial Exam ToEntity(UpdateExamDto dto);

    // ───── Exam sessions ─────
    public partial ExamSessionDto ToDto(ExamSession entity);
    public partial IQueryable<ExamSessionDto> ProjectToDto(IQueryable<ExamSession> query);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public partial ExamSession ToEntity(CreateExamSessionDto dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public partial ExamSession ToEntity(UpdateExamSessionDto dto);

    // ───── Menus ─────
    public partial MenuDto ToDto(Menu entity);
    public partial IQueryable<MenuDto> ProjectToDto(IQueryable<Menu> query);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public partial Menu ToEntity(CreateMenuDto dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public partial Menu ToEntity(UpdateMenuDto dto);

    // ───── Users ─────
    [MapperIgnoreTarget(nameof(UserSelectDto.FirstName))]
    [MapperIgnoreTarget(nameof(UserSelectDto.LastName))]
    [MapperIgnoreTarget(nameof(UserSelectDto.FatherName))]
    [MapperIgnoreTarget(nameof(UserSelectDto.NationalCode))]
    [MapperIgnoreTarget(nameof(UserSelectDto.Gender))]
    [MapperIgnoreTarget(nameof(UserSelectDto.MaritalStatus))]
    [MapperIgnoreTarget(nameof(UserSelectDto.Religion))]
    [MapperIgnoreTarget(nameof(UserSelectDto.LeftHanded))]
    [MapperIgnoreTarget(nameof(UserSelectDto.Nationality))]
    [MapperIgnoreTarget(nameof(UserSelectDto.Roles))]
    [MapperIgnoreTarget(nameof(UserSelectDto.Universities))]
    private partial UserSelectDto MapUserCore(User entity);

    public UserSelectDto ToDto(User source)
    {
        var target = MapUserCore(source);
        var profile = source.UserProfile?.FirstOrDefault();
        if (profile is not null)
        {
            target.FirstName = profile.FirstName;
            target.LastName = profile.LastName;
            target.FatherName = profile.FatherName;
            target.NationalCode = profile.NationalCode;
            target.Gender = profile.Gender;
            target.MaritalStatus = profile.MaritalStatus;
            target.Religion = profile.Religion;
            target.LeftHanded = profile.LeftHanded;
            target.Nationality = profile.Nationality;
        }

        target.Roles = source.UserRoles?
            .Where(x => x.Role != null)
            .Select(x => new RoleDto { Id = x.Role.Id, NameEn = x.Role.RoleNameEn, NameFa = x.Role.RoleNameFa })
            .ToList() ?? new List<RoleDto>();

        target.Universities = source.UniversityAccesses?
            .Where(x => x.IsActive && !x.IsDeleted && x.University != null)
            .Select(x => new UserUniversityDto { Id = x.UniversityId, Title = x.University.Title })
            .ToList() ?? new List<UserUniversityDto>();

        return target;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(CreateUserDto.Password))]
    [MapperIgnoreSource(nameof(CreateUserDto.FirstName))]
    [MapperIgnoreSource(nameof(CreateUserDto.LastName))]
    [MapperIgnoreSource(nameof(CreateUserDto.FatherName))]
    [MapperIgnoreSource(nameof(CreateUserDto.NationalCode))]
    [MapperIgnoreSource(nameof(CreateUserDto.Gender))]
    [MapperIgnoreSource(nameof(CreateUserDto.MaritalStatus))]
    [MapperIgnoreSource(nameof(CreateUserDto.Religion))]
    [MapperIgnoreSource(nameof(CreateUserDto.LeftHanded))]
    [MapperIgnoreSource(nameof(CreateUserDto.Nationality))]
    [MapperIgnoreSource(nameof(CreateUserDto.RoleIds))]
    public partial User ToEntity(CreateUserDto dto);
}
