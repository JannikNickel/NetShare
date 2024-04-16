using NetShare.Models;
using System;
using System.Collections.Generic;

namespace NetShare.Services
{
    public interface ISearchListenerService : IProcessService
    {
        event Action<IReadOnlyCollection<TransferTarget>>? TargetsChanged;
    }
}
