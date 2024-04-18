using NetShare.Models;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace NetShare.Services
{
    public class TcpSendContentService(ISettingsService settingsService) : ISendContentService
    {
        private readonly ISettingsService settingsService = settingsService;
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        private bool isRunning;
        private TcpClient? client;
        private DispatcherLimiter? progressDispatcher;
        private CancellationTokenSource? cts;

        private TransferTarget? target;
        private FileCollection? content;

        public event Action<string>? Error;
        public event Action<TransferProgressEventArgs>? Progress;
        public event Action? Completed;

        public void SetTransferData(TransferTarget target, FileCollection content)
        {
            this.target = target;
            this.content = content;
        }

        public void Start()
        {
            if(target == null || content == null)
            {
                return;
            }
            if(!isRunning.SetIfChanged(true))
            {
                return;
            }
            cts = new CancellationTokenSource();
            BeginTransfer(target, content, cts.Token);
        }

        public void Stop()
        {
            if(!isRunning.SetIfChanged(false))
            {
                return;
            }
            cts?.Cancel();
            cts?.Dispose();
            client?.Dispose();
            progressDispatcher?.Dispose();
        }

        public void CancelTransfer()
        {
            Stop();
        }

        private async void BeginTransfer(TransferTarget target, FileCollection content, CancellationToken ct)
        {
            try
            {
                progressDispatcher = new DispatcherLimiter(dispatcher, IContentTransferService.progressUpdateRate);
                using(client = new TcpClient())
                {
                    await client.ConnectAsync(target.Ip, settingsService.CurrentSettings?.TransferPort ?? 0, ct);
                    using TransferProtocol protocol = new TransferProtocol(client.GetStream());

                    string senderName = $"{settingsService.CurrentSettings?.DisplayName ?? "Unknown"} ({TransferTarget.GetLocalIp()?.ToString()})";
                    TransferReqInfo reqInfo = new TransferReqInfo(senderName, content.EntryCount, content.TotalSize);
                    TransferMessage msg = new TransferMessage(TransferMessage.Type.RequestTransfer, TransferReqInfo.Serialize(reqInfo));
                    await protocol.SendAsync(msg);

                    TransferMessage res = await protocol.ReadAsync(ct);
                    if(res.type != TransferMessage.Type.AcceptReceive)
                    {
                        HandleError(res.type == TransferMessage.Type.DeclineReceive ? "Transfer was declined by target!" : "Unexpected response!");
                        return;
                    }

                    int completed = 0;
                    long completedSize = 0;
                    string rootPath = content.RootPath;
                    Progress<long> subProgress = new Progress<long>(subTransferred => ReportProgress(completed, completedSize + subTransferred, protocol.TransferRate));
                    foreach(FileSystemInfo fsInfo in content.Entries)
                    {
                        string relPath = !string.IsNullOrEmpty(rootPath)
                            ? Path.GetRelativePath(rootPath, fsInfo.FullName)
                            : fsInfo.FullName[(Path.GetPathRoot(fsInfo.FullName)?.Length ?? 0)..];
                        if(fsInfo is FileInfo file)
                        {
                            long fileSize = file.Length;                            
                            msg = new TransferMessage(TransferMessage.Type.File, relPath, fileSize);
                            await protocol.SendAsync(msg, file, subProgress, ct);
                            completedSize += fileSize;
                        }
                        else if(fsInfo is DirectoryInfo dir)
                        {
                            msg = new TransferMessage(TransferMessage.Type.Directory, relPath);
                            await protocol.SendAsync(msg, null, null, ct);
                        }
                        ReportProgress(++completed, completedSize, protocol.TransferRate);
                    }
                    ReportProgress(completed, completedSize, 0, true);

                    msg = new TransferMessage(TransferMessage.Type.Complete);
                    await protocol.SendAsync(msg, null, null, ct);

                    NetworkStream stream = client.GetStream();
                    stream.Flush();
                    client.Client.Shutdown(SocketShutdown.Send);
                    Memory<byte> buffer = new byte[128];
                    while(await stream.ReadAsync(buffer, ct) != 0)
                    {
                        continue;
                    }
                    client.Close();

                    await dispatcher.InvokeAsync(() => Completed?.Invoke(), DispatcherPriority.Send);
                    Stop();
                }
            }
            catch(Exception e)
            {
                if(e is not OperationCanceledException)
                {
                    HandleError($"Error ({e.Message})!");
                }
                return;
            }
        }

        private void ReportProgress(int completedFiles, long completedSize, long rate, bool now = false)
        {
            progressDispatcher?.Invoke(() =>
            {
                Progress?.Invoke(new TransferProgressEventArgs(completedFiles, completedSize, rate));
            }, now);
        }

        private void HandleError(string error)
        {
            dispatcher.Invoke(() =>
            {
                Stop();
                Error?.Invoke(error);
            });
        }
    }
}
