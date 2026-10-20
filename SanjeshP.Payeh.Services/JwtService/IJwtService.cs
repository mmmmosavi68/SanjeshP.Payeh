using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Services.JwtService
{
    public interface IJwtService
    {
        Task<AccessToken> GenerateAsync(User user);
        Task<AccessToken> GenerateAsync(UserToken userToken);
    }
}