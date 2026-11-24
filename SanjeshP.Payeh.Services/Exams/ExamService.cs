using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data.Contracts.Exams;
using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Services.Exams;

public sealed class ExamService : IExamService, IScopedDependency
{
    private readonly IExamRepository _repository;
    private readonly ICodesService _codes;

    public ExamService(IExamRepository repository, ICodesService codes)
    {
        _codes = codes;
        _repository = repository;
    }

    public Task<List<Exam>> GetAllAsync(
        string? search,
        Guid? examSessionId,
        int? examTypeId,
        int? examModeId,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(
            search,
            examSessionId,
            examTypeId,
            examModeId,
            isActive,
            cancellationToken);
    }

    public async Task<Exam> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var exam = await _repository.GetByIdAsync(id, cancellationToken);

        if (exam is null)
            throw new NotFoundException(ApiMessages.Exam.M001);

        return exam;
    }

    public async Task<Exam> CreateAsync(
        Exam exam,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        if (createdBy == Guid.Empty)
            throw new BadRequestException(ApiMessages.Exam.M002);

        ValidateFields(exam);

        await ValidateReferencesAsync(exam, cancellationToken);

        exam.Title = exam.Title.Trim();
        exam.Description = exam.Description?.Trim();
        exam.CreatedBy = createdBy;
        exam.CreatedDate = DateTime.UtcNow;
        exam.UpdatedDate = null;
        exam.IsActive = true;
        exam.IsDeleted = false;

        await _repository.AddAsync(exam, cancellationToken);

        return await GetByIdAsync(exam.Id, cancellationToken);
    }

    public async Task<Exam> UpdateAsync(
        Guid id,
        Exam updatedExam,
        CancellationToken cancellationToken)
    {
        var exam = await GetByIdAsync(id, cancellationToken);

        ValidateFields(updatedExam);
        await ValidateReferencesAsync(updatedExam, cancellationToken);

        exam.Title = updatedExam.Title.Trim();
        exam.ExamSessionId = updatedExam.ExamSessionId;
        exam.ExamTypeId = updatedExam.ExamTypeId;
        exam.ExamModeId = updatedExam.ExamModeId;
        exam.RegistrationStartDate = updatedExam.RegistrationStartDate;
        exam.RegistrationEndDate = updatedExam.RegistrationEndDate;
        exam.EditStartDate = updatedExam.EditStartDate;
        exam.EditEndDate = updatedExam.EditEndDate;
        exam.Description = updatedExam.Description?.Trim();
        exam.IsActive = updatedExam.IsActive;
        exam.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(exam, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var exam = await GetByIdAsync(id, cancellationToken);

        if (await _repository.HasDependenciesAsync(id, cancellationToken))
        {
            throw new BadRequestException(
                ApiMessages.Exam.M003);
        }

        exam.IsDeleted = true;
        exam.IsActive = false;
        exam.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(exam, cancellationToken);
    }

    private static void ValidateFields(Exam exam)
    {
        if (string.IsNullOrWhiteSpace(exam.Title))
            throw new BadRequestException(ApiMessages.Exam.M004);

        if (exam.Title.Trim().Length > 200)
            throw new BadRequestException(ApiMessages.Exam.M005);

        if (exam.ExamSessionId == Guid.Empty)
            throw new BadRequestException(ApiMessages.Exam.M006);

        if (exam.ExamTypeId <= 0)
            throw new BadRequestException(ApiMessages.Exam.M007);

        if (exam.ExamModeId <= 0)
            throw new BadRequestException(ApiMessages.Exam.M008);

        if (exam.RegistrationEndDate < exam.RegistrationStartDate)
            throw new BadRequestException(ApiMessages.Exam.M009);

        if (exam.EditStartDate.HasValue != exam.EditEndDate.HasValue)
            throw new BadRequestException(ApiMessages.Exam.M010);

        if (exam.EditStartDate.HasValue &&
            exam.EditEndDate!.Value < exam.EditStartDate.Value)
            throw new BadRequestException(ApiMessages.Exam.M011);

        if (exam.Description?.Length > 1000)
            throw new BadRequestException(ApiMessages.Exam.M012);
    }

    private async Task ValidateReferencesAsync(
        Exam exam,
        CancellationToken cancellationToken)
    {
        if (!await _repository.SessionExistsAsync(
                exam.ExamSessionId, cancellationToken))
        {
            throw new BadRequestException(
                ApiMessages.Exam.M013);
        }

        await _codes.ResolveIdAsync(exam.ExamTypeId, null, null, CodeGroupTitles.ExamType, ApiMessages.Lookup.ExamTypeInvalid, cancellationToken);

        await _codes.ResolveIdAsync(exam.ExamModeId, null, null, CodeGroupTitles.ExamMode, ApiMessages.Lookup.ExamModeInvalid, cancellationToken);
    }

    public Task<PagedItems<TDto>> GetPageAsync<TDto>(string? search, Guid? examSessionId, int? examTypeId, int? examTypePrimaryCode, int? examTypeSubCode, int? examModeId, int? examModePrimaryCode, int? examModeSubCode, bool? isActive, int? page, int? pageSize, Func<IQueryable<Exam>, IQueryable<TDto>> project, CancellationToken cancellationToken)
    {
        return _repository.GetPageAsync(search, examSessionId, examTypeId, examTypePrimaryCode, examTypeSubCode, examModeId, examModePrimaryCode, examModeSubCode, isActive, page, pageSize, project, cancellationToken);
    }
}