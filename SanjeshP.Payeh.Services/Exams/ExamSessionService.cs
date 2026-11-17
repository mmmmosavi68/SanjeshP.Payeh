
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Data.Contracts.Exams;
using SanjeshP.Payeh.Entities.Exam;

namespace SanjeshP.Payeh.Services.Exams;

public sealed class ExamSessionService : IExamSessionService, IScopedDependency
{
    private readonly IExamSessionRepository _repository;

    public ExamSessionService(IExamSessionRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ExamSession>> GetAllAsync(
        string? search,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(
            search,
            isActive,
            cancellationToken);
    }

    public async Task<ExamSession> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (session is null)
            throw new NotFoundException("نوبت آزمون موردنظر پیدا نشد.");

        return session;
    }

    public async Task<ExamSession> CreateAsync(
        ExamSession session,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        Validate(session);

        if (createdBy == Guid.Empty)
            throw new BadRequestException("شناسه کاربر ثبت‌کننده معتبر نیست.");

        session.Title = session.Title.Trim();
        session.CreatedBy = createdBy;
        session.CreatedDate = DateTime.UtcNow;
        session.UpdatedDate = null;
        session.IsActive = true;
        session.IsDeleted = false;

        await _repository.AddAsync(session, cancellationToken);

        return await GetByIdAsync(session.Id, cancellationToken);
    }

    public async Task<ExamSession> UpdateAsync(
        Guid id,
        ExamSession updatedSession,
        CancellationToken cancellationToken)
    {
        var session = await GetByIdAsync(id, cancellationToken);

        Validate(updatedSession);

        session.Title = updatedSession.Title.Trim();
        session.ExamDate = updatedSession.ExamDate;
        session.StartTime = updatedSession.StartTime;
        session.EndTime = updatedSession.EndTime;
        session.IsActive = updatedSession.IsActive;
        session.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(session, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var session = await GetByIdAsync(id, cancellationToken);

        if (await _repository.HasExamsAsync(id, cancellationToken))
        {
            throw new BadRequestException(
                "برای این نوبت آزمون تعریف شده است و امکان حذف آن وجود ندارد.");
        }

        session.IsDeleted = true;
        session.IsActive = false;
        session.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(session, cancellationToken);
    }

    private static void Validate(ExamSession session)
    {
        if (string.IsNullOrWhiteSpace(session.Title))
            throw new BadRequestException("عنوان نوبت الزامی است.");

        if (session.Title.Trim().Length > 100)
            throw new BadRequestException("عنوان نوبت نباید بیشتر از ۱۰۰ کاراکتر باشد.");

        if (session.EndTime <= session.StartTime)
            throw new BadRequestException("ساعت پایان باید بعد از ساعت شروع باشد.");
    }
}
