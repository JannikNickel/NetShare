using NetShare.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace NetShare.Services
{
    public class TcpReceiveContentService : IReceiveContentService
    {
        private readonly ISettingsService settingsService;
        private bool isRunning;
        private TcpListener? server;
        private Dispatcher dispatcher;
        private DispatcherLimiter? progressDispatcher;
        private CancellationTokenSource? cts;
        private Func<TransferReqInfo, bool>? confirmTransferCallback = null;

        public event Action<string>? Error;
        public event Action<TransferProgressEventArgs>? Progress;
        public event Action? Completed;
        public event Action<TransferReqInfo>? BeginTransfer;

        public TcpReceiveContentService(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Start()
        {
            if(!isRunning.SetIfChanged(true))
            {
                return;
            }
            server = new TcpListener(IPAddress.Any, settingsService.CurrentSettings?.TransferPort ?? 0);
            cts = new CancellationTokenSource();
            AwaitConnection(cts.Token);
        }

        public void Stop()
        {
            if(!isRunning.SetIfChanged(false))
            {
                return;
            }
            cts?.Cancel();
            cts?.Dispose();
            server?.Dispose();
            progressDispatcher?.Dispose();
        }

        public void SetConfirmTransferCallback(Func<TransferReqInfo, bool>? callback)
        {
            confirmTransferCallback = callback;
        }

        public void CancelTransfer()
        {
            cts?.Cancel();
            Stop();
        }

        private void Restart()
        {
            Stop();
            Start();
        }

        private async void AwaitConnection(CancellationToken ct)
        {
            if(server == null)
            {
                return;
            }

            server.Start();
            try
            {
                progressDispatcher = new DispatcherLimiter(dispatcher, IContentTransferService.progressUpdateRate);
                using(TcpClient client = await server.AcceptTcpClientAsync(ct))
                {
                    string? downloadPath = settingsService.CurrentSettings?.DownloadPath ?? null;
                    if(downloadPath == null || !Settings.PrepDownloadPath(ref downloadPath))
                    {
                        HandleError($"Can't write to download path ({downloadPath})!");
                        return;
                    }

                    using TransferProtocol protocol = new TransferProtocol(client);

                    TransferMessage msg;
                    msg = await protocol.ReadAsync(ct);
                    if(msg.type != TransferMessage.Type.RequestTransfer)
                    {
                        HandleError($"Unexpected initial message! Expected transfer request!");
                        return;
                    }
                    TransferReqInfo? reqInfo = TransferReqInfo.Deserialize(msg.path);
                    if(reqInfo == null)
                    {
                        HandleError($"Missing request info!");
                        return;
                    }

                    TransferMessage.Type reqAnswer = TransferMessage.Type.AcceptReceive;
                    await dispatcher.InvokeAsync(() =>
                    {
                        if(confirmTransferCallback != null)
                        {
                            reqAnswer = confirmTransferCallback.Invoke(reqInfo.Value) ? TransferMessage.Type.AcceptReceive : TransferMessage.Type.DeclineReceive;
                        }
                    }, DispatcherPriority.Send, ct);
                    msg = new TransferMessage(reqAnswer);
                    await protocol.SendAsync(msg);
                    if(reqAnswer == TransferMessage.Type.DeclineReceive)
                    {
                        Restart();
                        return;
                    }

                    await dispatcher.InvokeAsync(() => BeginTransfer?.Invoke(reqInfo.Value), DispatcherPriority.Send);

                    int completed = 0;
                    long received = 0;
                    Progress<long> subProgress = new Progress<long>(subReceived => ReportProgress(completed, received + subReceived, protocol.ReceiveRate));
                    do
                    {
                        msg = await protocol.ReadAsync(ct);
                        if(msg.type == TransferMessage.Type.File)
                        {
                            string path = Path.Combine(downloadPath, msg.path ?? "");
                            string? dir = Path.GetDirectoryName(path);
                            if(dir != null && !Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            received += await protocol.ReadData(path, msg, subProgress, ct);
                            completed++;
                            ReportProgress(completed, received, protocol.ReceiveRate);

                            await protocol.SendAsync(new TransferMessage(TransferMessage.Type.Continue));
                        }
                        else
                        {
                            ReportProgress(completed, received, 0, true);

                            if(msg.type == TransferMessage.Type.Cancel)
                            {
                                HandleError("Transfer cancelled by sender!");
                                return;
                            }
                            else if(msg.type == TransferMessage.Type.Complete)
                            {
                                NetworkStream stream = client.GetStream();
                                stream.Flush();
                                Memory<byte> buffer = new byte[128];
                                while(await stream.ReadAsync(buffer, ct) != 0)
                                {

                                }
                                client.Client.Shutdown(SocketShutdown.Send);
                                client.Close();

                                await dispatcher.InvokeAsync(() => Completed?.Invoke(), DispatcherPriority.Send);
                                break;
                            }

                            HandleError($"Unexpected message ({msg.type})");
                        }
                    }
                    while(msg.type != TransferMessage.Type.None);
                }
                Stop();
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
