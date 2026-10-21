using SanjeshP.Payeh.Entities.User;

namespace SanjeshP.Payeh.Data.Contracts
{
    public interface IUserTokenRepository : IEFRepository<UserToken>
    {
        Task<UserToken?> GetByJtiAsync(string jti, CancellationToken cancellationToken);
        Task<List<UserToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}