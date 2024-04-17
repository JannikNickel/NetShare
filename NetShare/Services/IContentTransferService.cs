using System;

namespace NetShare.Services
{
    public interface IContentTransferService : IProcessService
    {
        public const double progressUpdateRate = 1.0 / 30.0;

        event Action<string>? Error;
        event Action<TransferProgressEventArgs>? Progress;
        event Action? Completed;

        void CancelTransfer();
    }

    public record struct TransferProgressEventArgs(int FilesCompleted, long BytesCompleted, long Rate);
}
