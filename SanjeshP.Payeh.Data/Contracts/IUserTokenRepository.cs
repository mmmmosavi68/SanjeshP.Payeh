using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Data.Contracts
{
    public interface IUserTokenRepository : IEFRepository<UserToken>
    {
        Task<UserToken> CreateToken(User user, bool allowMultipleSessions, CancellationToken cancellationToken);
    }
}
