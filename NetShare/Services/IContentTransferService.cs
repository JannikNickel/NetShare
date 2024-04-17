using System;

namespace NetShare.Services
{
    public interface IContentTransferService : IProcessService
    {
        event Action<string>? Error;
        event Action<TransferProgressEventArgs>? Progress;
        event Action? Completed;
    }

    public record struct TransferProgressEventArgs(int FilesCompleted, long BytesCompleted, long Rate);
}
