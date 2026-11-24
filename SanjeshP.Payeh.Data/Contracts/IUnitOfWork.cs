using SanjeshP.Payeh.Entities.Common;

namespace SanjeshP.Payeh.Data.Contracts;

/// <summary>نقطه‌ی واحد دسترسی لایه Services به داده؛ فقط از طریق Repository و ذخیره‌ی یکپارچه.</summary>
public interface IUnitOfWork : SanjeshP.Payeh.Common.IScopedDependency
{
    IEFRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
}
