using Microsoft.EntityFrameworkCore;
using SanjeshP.Payeh.Common;
using SanjeshP.Payeh.Data.Contracts;
using SanjeshP.Payeh.Entities.Menu;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Data.Repositories
{
    public sealed class CodesService : ICodesService, IScopedDependency
    {
        private readonly IEFRepository<Code> _repository;

        public CodesService(IEFRepository<Code> repository)
        {
            _repository = repository;
        }

        public async Task<List<Code>> GetGroupsAsync(
            CancellationToken cancellationToken)
        {
            var root = _repository.TableNoTracking
                .Where(x =>
                    x.ParentId == null &&
                    x.PrimaryCode == 0 &&
                    x.SubCode == 0 &&
                    x.IsActive &&
                    !x.IsDeleted)
                .FirstOrDefault();

            if (root is null)
                return [];

            return _repository.TableNoTracking
                .Where(x =>
                    x.ParentId == root.Id &&
                    x.IsActive &&
                    !x.IsDeleted)
                .OrderBy(x => x.PrimaryCode)
                .ThenBy(x => x.Title)
                .ToList();
        }


        public async Task<List<Code>> GetItemsAsync(
            int primaryCode,
            CancellationToken cancellationToken)
        {
            var group = await _repository.TableNoTracking
                .Where(x =>
                    x.PrimaryCode == primaryCode &&
                    x.SubCode == 0 &&
                    x.ParentId != null &&
                    x.IsActive &&
                    !x.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (group is null)
                return [];

            return await _repository.TableNoTracking
                .Where(x =>
                    x.ParentId == group.Id &&
                    x.PrimaryCode == primaryCode &&
                    x.SubCode > 1 &&
                    x.IsActive &&
                    !x.IsDeleted)
                .OrderBy(x => x.SubCode)
                .ThenBy(x => x.Title)
                .ToListAsync(cancellationToken);
        }

    }
}