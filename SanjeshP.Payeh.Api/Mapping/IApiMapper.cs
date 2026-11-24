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

/// <summary>نقطه‌ی واحد نگاشت Entity ↔ DTO (Mapperly؛ اعتبارسنجی در زمان Build).</summary>
public interface IApiMapper
{
    CodeDto ToDto(Code entity);
    IQueryable<CodeDto> ProjectToDto(IQueryable<Code> query);
    Code ToEntity(CreateCodeDto dto);
    Code ToEntity(UpdateCodeDto dto);

    UniversityDto ToDto(University entity);
    IQueryable<UniversityDto> ProjectToDto(IQueryable<University> query);
    University ToEntity(CreateUniversityDto dto);
    University ToEntity(UpdateUniversityDto dto);

    ExamDto ToDto(Exam entity);
    IQueryable<ExamDto> ProjectToDto(IQueryable<Exam> query);
    Exam ToEntity(CreateExamDto dto);
    Exam ToEntity(UpdateExamDto dto);

    ExamSessionDto ToDto(ExamSession entity);
    IQueryable<ExamSessionDto> ProjectToDto(IQueryable<ExamSession> query);
    ExamSession ToEntity(CreateExamSessionDto dto);
    ExamSession ToEntity(UpdateExamSessionDto dto);

    MenuDto ToDto(Menu entity);
    IQueryable<MenuDto> ProjectToDto(IQueryable<Menu> query);
    Menu ToEntity(CreateMenuDto dto);
    Menu ToEntity(UpdateMenuDto dto);

    UserSelectDto ToDto(User entity);
    User ToEntity(CreateUserDto dto);
}
