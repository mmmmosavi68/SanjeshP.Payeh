
namespace SanjeshP.Payeh.Services.Validations;

public interface IIdentityValidationService
{
    Task<string> ValidateUserNationalCodeAsync(
        string? nationalCode,
        int? excludeProfileId,
        CancellationToken cancellationToken);

    Task<string?> ValidateUserMobileNumberAsync(
        string? mobileNumber,
        Guid? excludeUserId,
        CancellationToken cancellationToken);

    Task<string> ValidateExamRegistrationNationalCodeAsync(
        Guid examId,
        string? nationalCode,
        Guid? excludeRegistrationId,
        CancellationToken cancellationToken);

    Task<string?> ValidateExamRegistrationMobileNumberAsync(
        Guid examId,
        string? mobileNumber,
        Guid? excludeRegistrationId,
        CancellationToken cancellationToken);
}
