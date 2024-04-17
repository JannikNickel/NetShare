using NetShare.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetShare.Services
{
    public interface ISendContentService : IContentTransferService
    {
        void SetTransferData(TransferTarget target, FileCollection content);
    }
}
