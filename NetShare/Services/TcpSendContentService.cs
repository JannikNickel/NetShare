﻿using NetShare.Models;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace NetShare.Services
{
    public class TcpSendContentService : ISendContentService
    {
        private ISettingsService settingsService;
        private bool isRunning;
        private TcpClient? client;
        private Dispatcher dispatcher;
        private CancellationTokenSource? cts;

        private TransferTarget? target;
        private FileCollection? content;

        public event Action<string>? Error;
        public event Action<TransferProgressEventArgs>? Progress;
        public event Action? Completed;

        public TcpSendContentService(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

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
        }

        private async void BeginTransfer(TransferTarget target, FileCollection content, CancellationToken ct)
        {
            try
            {
                using(client = new TcpClient())
                {
                    await client.ConnectAsync(target.Ip, settingsService.CurrentSettings?.TransferPort ?? 0, ct);

                    using TransferProtocol protocol = new TransferProtocol(client);

                    TransferReqInfo reqInfo = new TransferReqInfo($"{settingsService.CurrentSettings?.DisplayName ?? "Unknown"} ({TransferTarget.GetLocalIp()?.ToString()})", content.EntryCount, content.TotalSize);
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
                    foreach(FileInfo file in content.Entries)
                    {
                        long fileSize = file.Length;
                        string relPath = !string.IsNullOrEmpty(rootPath)
                            ? Path.GetRelativePath(rootPath, file.FullName)
                            : file.FullName[(Path.GetPathRoot(file.FullName)?.Length ?? 0)..];
                        msg = new TransferMessage(TransferMessage.Type.File, relPath, fileSize);
                        await protocol.SendAsync(msg, file, subProgress, ct);
                        completed++;
                        completedSize += fileSize;
                        ReportProgress(completed, completedSize, protocol.TransferRate);
                    }

                    msg = new TransferMessage(TransferMessage.Type.Complete);
                    await protocol.SendAsync(msg, null, null, ct);

                    NetworkStream stream = client.GetStream();
                    stream.Flush();
                    client.Client.Shutdown(SocketShutdown.Send);
                    Memory<byte> buffer = new byte[128];
                    while(await stream.ReadAsync(buffer, ct) != 0)
                    {

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
                    HandleError($"Could not establish connection ({e.Message})!");
                }
                return;
            }
        }

        private void ReportProgress(int completedFiles, long completedSize, long rate)
        {
            dispatcher.Invoke(() =>
            {
                Progress?.Invoke(new TransferProgressEventArgs(completedFiles, completedSize, rate));
            });
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
