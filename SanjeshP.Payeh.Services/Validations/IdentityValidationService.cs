
using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Common.Exceptions;
using SanjeshP.Payeh.Common.Utilities;
using SanjeshP.Payeh.Data;
using SanjeshP.Payeh.Entities.Exam;
using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.Validations;

public sealed class IdentityValidationService
    : IIdentityValidationService, IScopedDependency
{
    private readonly IUnitOfWork _uow;

    public IdentityValidationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<string> ValidateUserNationalCodeAsync(
        string? nationalCode,
        int? excludeProfileId,
        CancellationToken cancellationToken)
    {
        var normalizedCode = NormalizeNationalCode(nationalCode);

        var exists = await _uow.Repository<UserProfile>().Entities
            .AnyAsync(
                x => x.NationalCode == normalizedCode &&
                     (!excludeProfileId.HasValue || x.Id != excludeProfileId.Value),
                cancellationToken);

        if (exists)
            throw new AppException(
                ApiResultStatusCode.Conflict,
                "این کد ملی قبلاً برای کاربر دیگری ثبت شده است.");

        return normalizedCode;
    }

    public async Task<string?> ValidateUserMobileNumberAsync(
        string? mobileNumber,
        Guid? excludeUserId,
        CancellationToken cancellationToken)
    {
        var normalizedNumber = NormalizeMobileNumber(mobileNumber);

        if (normalizedNumber is null)
            return null;

        var exists = await _uow.Repository<User>().Entities
            .AnyAsync(
                x => x.PhoneNumber == normalizedNumber &&
                     (!excludeUserId.HasValue || x.Id != excludeUserId.Value),
                cancellationToken);

        if (exists)
            throw new AppException(
                ApiResultStatusCode.Conflict,
                "این شماره همراه قبلاً برای کاربر دیگری ثبت شده است.");

        return normalizedNumber;
    }

    public async Task<string> ValidateExamRegistrationNationalCodeAsync(
        Guid examId,
        string? nationalCode,
        Guid? excludeRegistrationId,
        CancellationToken cancellationToken)
    {
        if (examId == Guid.Empty)
            throw new BadRequestException("شناسه آزمون معتبر نیست.");

        var normalizedCode = NormalizeNationalCode(nationalCode);

        var exists = await _uow.Repository<ExamRegistration>().Entities
            .AnyAsync(
                x => x.ExamId == examId &&
                     x.NationalCode == normalizedCode &&
                     (!excludeRegistrationId.HasValue ||
                      x.Id != excludeRegistrationId.Value),
                cancellationToken);

        if (exists)
            throw new AppException(
                ApiResultStatusCode.Conflict,
                "این کد ملی قبلاً در این آزمون ثبت‌نام کرده است.");

        return normalizedCode;
    }

    public async Task<string?> ValidateExamRegistrationMobileNumberAsync(
        Guid examId,
        string? mobileNumber,
        Guid? excludeRegistrationId,
        CancellationToken cancellationToken)
    {
        if (examId == Guid.Empty)
            throw new BadRequestException("شناسه آزمون معتبر نیست.");

        var normalizedNumber = NormalizeMobileNumber(mobileNumber);

        if (normalizedNumber is null)
            return null;

        var exists = await _uow.Repository<ExamRegistration>().Entities
            .AnyAsync(
                x => x.ExamId == examId &&
                     x.PhoneNumber == normalizedNumber &&
                     (!excludeRegistrationId.HasValue ||
                      x.Id != excludeRegistrationId.Value),
                cancellationToken);

        if (exists)
            throw new AppException(
                ApiResultStatusCode.Conflict,
                "این شماره همراه قبلاً در این آزمون ثبت شده است.");

        return normalizedNumber;
    }

    private static string NormalizeNationalCode(string? nationalCode)
    {
        var normalizedCode = nationalCode?.Fa2En().Trim();

        if (!IranianNationalCodeValidator.IsValid(normalizedCode))
            throw new BadRequestException("کد ملی واردشده معتبر نیست.");

        return normalizedCode!;
    }

    private static string? NormalizeMobileNumber(string? mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
            return null;

        if (!IranianMobileNumberNormalizer.TryNormalize(
                mobileNumber,
                out var normalizedNumber))
        {
            throw new BadRequestException("شماره همراه واردشده معتبر نیست.");
        }

        return normalizedNumber;
    }
}
