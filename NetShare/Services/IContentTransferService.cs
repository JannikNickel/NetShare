using System;

namespace NetShare.Services
{
    public interface IContentTransferService : IProcessService
    {
        event Action<string>? Error;
        event Action? Completed;
    }
}
