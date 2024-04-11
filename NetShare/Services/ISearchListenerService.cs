using NetShare.Models;
using System;
using System.Collections.Generic;

namespace NetShare.Services
{
    public interface ISearchListenerService : IBroadcastSearchService
    {
        event Action<IReadOnlyCollection<TransferTarget>>? TargetsChanged;
    }
}
