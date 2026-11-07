using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Audit;

namespace SanjeshP.Payeh.Data.Repositories;

/// <summary>پیاده‌سازی جستجوی سوابق ممیزی با فیلترهای امن و صفحه‌بندی.</summary>
public sealed class AuditLogQueryService : IAuditLogQueryService, IScopedDependency
{
    private readonly IEFRepository<AuditLog> _repository;

    public AuditLogQueryService(IEFRepository<AuditLog> repository)
    {
        _repository = repository;
    }

    public async Task<PagedAuditLogs> SearchAsync(AuditLogQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<AuditLog> records = _repository.TableNoTracking;

        if (query.FromUtc.HasValue)
            records = records.Where(x => x.OccurredAtUtc >= query.FromUtc.Value);

        if (query.ToUtc.HasValue)
            records = records.Where(x => x.OccurredAtUtc <= query.ToUtc.Value);

        if (query.UserId.HasValue)
            records = records.Where(x => x.UserId == query.UserId.Value);

        if (!string.IsNullOrWhiteSpace(query.UserName))
        {
            var value = query.UserName.Trim();
            records = records.Where(x => x.UserName != null && x.UserName.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(query.IpAddress))
        {
            var value = query.IpAddress.Trim();
            records = records.Where(x => x.IpAddress != null && x.IpAddress.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            var value = query.EventType.Trim();
            records = records.Where(x => x.EventType == value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var value = query.Action.Trim();
            records = records.Where(x => x.Action.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var value = query.EntityName.Trim();
            records = records.Where(x => x.EntityName == value);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            var value = query.EntityId.Trim();
            records = records.Where(x => x.EntityId == value);
        }

        if (query.StatusCode.HasValue)
            records = records.Where(x => x.StatusCode == query.StatusCode.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var value = query.Search.Trim();
            records = records.Where(x =>
                (x.UserName != null && x.UserName.Contains(value)) ||
                (x.Description != null && x.Description.Contains(value)) ||
                x.Action.Contains(value) ||
                (x.Path != null && x.Path.Contains(value)) ||
                (x.EntityName != null && x.EntityName.Contains(value)) ||
                (x.EntityId != null && x.EntityId.Contains(value)) ||
                (x.IpAddress != null && x.IpAddress.Contains(value)));
        }

        var totalCount = await records.CountAsync(cancellationToken);
        var items = await records
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedAuditLogs(items, totalCount, pageNumber, pageSize);
    }
}
