using NetShare.Models;
using System;
using System.Threading.Tasks;

namespace NetShare.Services
{
    public interface IReceiveContentService : IContentTransferService
    {
        event Action<TransferReqInfo>? BeginTransfer;

        void SetConfirmTransferCallback(Func<TransferReqInfo, Task<bool>>? callback);
    }
}
