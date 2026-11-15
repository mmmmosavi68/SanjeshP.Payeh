using SanjeshP.Payeh.Entities.Menu;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanjeshP.Payeh.Data.Contracts
{

    public interface ICodesService
    {
        Task<List<Code>> GetGroupsAsync(
            CancellationToken cancellationToken);

        Task<List<Code>> GetItemsAsync(
            int primaryCode,
            CancellationToken cancellationToken);
    }
}