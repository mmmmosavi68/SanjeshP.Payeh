using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;

namespace SanjeshP.Payeh.Data.Repositories;

public static class PagingExtensions
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 500;

    /// <summary>پروجکشن تایپ‌شده + صفحه‌بندی اختیاری. کوئری ورودی باید مرتب‌شده باشد.</summary>
    public static async Task<PagedItems<TDto>> ToPagedAsync<TEntity, TDto>(this IQueryable<TEntity> orderedQuery, Func<IQueryable<TEntity>, IQueryable<TDto>> project, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        if (page is null && pageSize is null)
        {
            var all = await project(orderedQuery).ToListAsync(cancellationToken);
            return new PagedItems<TDto>(all, all.Count, 1, all.Count, false);
        }

        var currentPage = Math.Max(page ?? 1, 1);
        var currentSize = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);
        var total = await orderedQuery.CountAsync(cancellationToken);
        var items = await project(orderedQuery.Skip((currentPage - 1) * currentSize).Take(currentSize)).ToListAsync(cancellationToken);
        return new PagedItems<TDto>(items, total, currentPage, currentSize, true);
    }
}
