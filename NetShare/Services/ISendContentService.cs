using NetShare.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetShare.Services
{
    public interface ISendContentService : IProcessService
    {
        event Action<string>? Error;
        event Action<int, long>? Progress;

        void SetTransferData(TransferTarget target, FileCollection content);
    }
}
