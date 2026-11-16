namespace SanjeshP.Payeh.Services.Validations;

public interface IIdentityValidationService
{
    Task ValidateNationalCodeAsync(
        string nationalCode,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task ValidateMobileNumberAsync(
        string? mobileNumber,
        Guid? excludeId,
        CancellationToken cancellationToken);

    Task<string?> NormalizeMobileNumberAsync(
        string? mobileNumber);
}