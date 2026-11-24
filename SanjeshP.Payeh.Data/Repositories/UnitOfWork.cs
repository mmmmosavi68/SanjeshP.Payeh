using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Common;

namespace SanjeshP.Payeh.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public IEFRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity
    {
        if (!_repositories.TryGetValue(typeof(TEntity), out var repository))
        {
            repository = new EFRepository<TEntity>(_dbContext);
            _repositories[typeof(TEntity)] = repository;
        }
        return (IEFRepository<TEntity>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }
}
