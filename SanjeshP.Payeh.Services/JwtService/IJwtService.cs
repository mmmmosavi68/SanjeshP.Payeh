using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.JwtService
{
    public interface IJwtService
    {
        Task<AccessToken> GenerateAsync(User user, string deviceName, string ipAddress, CancellationToken cancellationToken);
        Task RevokeAsync(string jti, CancellationToken cancellationToken);
        Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken);
        Task<bool> ValidateTokenAsync(string jti, Guid userId, CancellationToken cancellationToken);
    }
}