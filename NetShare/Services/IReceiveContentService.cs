using NetShare.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetShare.Services
{
    public interface IReceiveContentService : IContentTransferService
    {
        event Action? BeginTransfer;

        void SetConfirmTransferCallback(Func<string, bool>? callback);
    }
}
