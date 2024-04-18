using NetShare.Models;

namespace NetShare.Services
{
    public interface ISendContentService : IContentTransferService
    {
        void SetTransferData(TransferTarget target, FileCollection content);
    }
}
