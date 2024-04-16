using NetShare.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetShare.Services
{
    public interface IReceiveContentService : IProcessService
    {
        event Action<string>? Error;
        event Action? BeginTransfer;
        event Action? Completed;

        void SetConfirmTransferCallback(Func<bool>? callback);
    }
}
