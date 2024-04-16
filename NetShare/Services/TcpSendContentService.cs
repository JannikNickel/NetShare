using NetShare.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public event Action<int, long>? Progress;

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
                client = new TcpClient();
                await client.ConnectAsync(target.Ip, settingsService.CurrentSettings?.TransferPort ?? 0, ct);

                TransferProtocol protocol = new TransferProtocol(client);
                await protocol.SendAsync(new TransferMessage(TransferMessage.Type.RequestTransfer));

                TransferMessage res = await protocol.ReadAsync(ct);
                if(res.type != TransferMessage.Type.AcceptReceive)
                {
                    HandleError(res.type == TransferMessage.Type.DeclineReceive ? "Transfer was declined by target!" : "Unexpected response!");
                    return;
                }

                long completedSize = 0;
                string rootPath = content.RootPath;
                foreach((int i, FileInfo file) in content.Entries.Enumerate())
                {
                    long fileSize = file.Length;
                    string relPath = !string.IsNullOrEmpty(rootPath)
                        ? Path.GetRelativePath(rootPath, file.FullName)
                        : file.FullName[(Path.GetPathRoot(file.FullName)?.Length ?? 0)..];
                    TransferMessage msg = new TransferMessage(TransferMessage.Type.File, relPath, fileSize);
                    await protocol.SendAsync(msg, file, ct);
                    completedSize += fileSize;
                    ReportProgress(i + 1, completedSize);
                }

                client.Dispose();
                return;
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

        private void ReportProgress(int completedFiles, long completedSize)
        {
            dispatcher.Invoke(() =>
            {
                Progress?.Invoke(completedFiles, completedSize);
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
