using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Api.DTOs.Codes;
using SanjeshP.Payeh.Api.DTOs.Exams;
using SanjeshP.Payeh.Api.Mapping;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.Menu;
using SanjeshP.Payeh.Entities.University;
using Xunit;

namespace SanjeshP.Payeh.Tests;

public class MappingTests
{
    private static readonly string[] DtoNamespaces = { "SanjeshP.Payeh.Api.DTOs.Codes", "SanjeshP.Payeh.Api.DTOs.Exams", "SanjeshP.Payeh.Api.DTOs.ExamSessions", "SanjeshP.Payeh.Api.DTOs.Menus", "SanjeshP.Payeh.Api.DTOs.Universities" };

    /// <summary>هر DTO جدید در این Namespaceها باید در IApiMapper نگاشت داشته باشد؛ در غیر این‌صورت تست قرمز می‌شود.</summary>
    [Fact]
    public void EveryDtoIsCoveredByApiMapper()
    {
        var mapped = typeof(IApiMapper).GetMethods().SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType)).Select(t => t.IsGenericType ? t.GetGenericArguments()[0] : t).ToHashSet();
        var dtos = typeof(IApiMapper).Assembly.GetTypes().Where(t => t.IsClass && t.Namespace is not null && DtoNamespaces.Contains(t.Namespace) && t.Name.EndsWith("Dto")).ToList();

        Assert.NotEmpty(dtos);
        Assert.All(dtos, dto => Assert.True(mapped.Contains(dto), $"DTO {dto.Name} has no mapping in IApiMapper"));
    }

    [Fact]
    public void ApiMapper_ImplementsEveryInterfaceMethod()
    {
        var implemented = typeof(ApiMapper).GetInterfaceMap(typeof(IApiMapper)).TargetMethods;
        Assert.All(implemented, m => Assert.NotNull(m));
    }

    [Fact]
    public void ToDto_IsNullSafe_WhenNavigationsAreNotLoaded()
    {
        IApiMapper mapper = new ApiMapper();

        var exam = new Exam { Title = "e", ExamTypeId = 3, ExamModeId = 4 };
        var dto = mapper.ToDto(exam);

        Assert.Equal(3, dto.ExamTypeId);
        Assert.Null(dto.ExamTypeTitle);
        Assert.Null(dto.ExamTypePrimaryCode);
        Assert.Null(dto.ExamModeSubCode);
        Assert.Equal(default, dto.ExamDate);

        var code = mapper.ToDto(new Code { Title = "c", PrimaryCode = 1, SubCode = 2 });
        Assert.Null(code.ParentPrimaryCode);
        Assert.Equal(2, code.SubCode);
    }

    [Fact]
    public void ToEntity_CopiesIdsAndIgnoresCodePairs()
    {
        IApiMapper mapper = new ApiMapper();

        var entity = mapper.ToEntity(new CreateExamDto { Title = "t", ExamTypeId = 5, ExamTypePrimaryCode = 9, ExamTypeSubCode = 8 });

        Assert.Equal("t", entity.Title);
        Assert.Equal(5, entity.ExamTypeId);
    }

    [Fact]
    public async Task ProjectToDto_ReturnsCodeFields_FromDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("projection-" + Guid.NewGuid()).Options;
        await using var db = new ApplicationDbContext(options);

        var group = new Code { Title = "g", PrimaryCode = 40, SubCode = 0 };
        var type = new Code { Title = "item", PrimaryCode = 40, SubCode = 2, Parent = group };
        var mode = new Code { Title = "mode", PrimaryCode = 41, SubCode = 5 };
        var session = new ExamSession { Title = "s", ExamDate = new DateTime(2030, 1, 1) };
        db.AddRange(group, type, mode, session);
        db.Add(new Exam { Title = "exam", ExamSession = session, ExamType = type, ExamMode = mode });
        db.Add(new University { Title = "uni", UniversityType = type });
        await db.SaveChangesAsync();

        IApiMapper mapper = new ApiMapper();
        var exam = await mapper.ProjectToDto(db.Set<Exam>().AsNoTracking()).SingleAsync();
        var codes = await mapper.ProjectToDto(db.Set<Code>().AsNoTracking()).ToListAsync();
        var university = await mapper.ProjectToDto(db.Set<University>().AsNoTracking()).SingleAsync();

        Assert.Equal(40, exam.ExamTypePrimaryCode);
        Assert.Equal(2, exam.ExamTypeSubCode);
        Assert.Equal(41, exam.ExamModePrimaryCode);
        Assert.Equal(new DateTime(2030, 1, 1), exam.ExamDate);
        Assert.Equal("s", exam.ExamSessionTitle);
        Assert.Equal(40, codes.Single(c => c.SubCode == 2).ParentPrimaryCode);
        Assert.Equal(2, university.UniversityTypeSubCode);
    }
}
