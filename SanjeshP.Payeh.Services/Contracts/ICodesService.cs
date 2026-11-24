using SanjeshP.Payeh.Entities.Menu;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Services.Contracts
{

    public interface ICodesService
    {
        // متدهای فعلی برای فرم اطلاعات کاربر
        Task<List<Code>> GetGroupsAsync(
            CancellationToken cancellationToken);

        Task<List<Code>> GetItemsAsync(
            int primaryCode,
            CancellationToken cancellationToken);

        // متدهای مدیریت کدها
        Task<List<Code>> GetAllAsync(
            CancellationToken cancellationToken);

        Task<Code> GetByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<Code> CreateAsync(
            Code code,
            Guid createdBy,
            CancellationToken cancellationToken);

        Task<Code> UpdateAsync(
            int id,
            Code updatedCode,
            CancellationToken cancellationToken);

        Task SoftDeleteAsync(
            int id,
            CancellationToken cancellationToken);

        Task<List<Code>> GetManagementGroupsAsync(
    CancellationToken cancellationToken);

        Task<List<Code>> GetManagementItemsAsync(
            int groupId,
            CancellationToken cancellationToken);

    Task<int> ResolveIdAsync(int? id, int? primaryCode, int? subCode, string? groupTitle, ApiMessage invalidMessage, CancellationToken cancellationToken);

    Task<PagedItems<TDto>> GetPageAsync<TDto>(int? page, int? pageSize, Func<IQueryable<Code>, IQueryable<TDto>> project, CancellationToken cancellationToken);
}
}