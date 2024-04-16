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
    public class TcpReceiveContentService : IReceiveContentService
    {
        private readonly ISettingsService settingsService;
        private bool isRunning;
        private TcpListener? server;
        private Dispatcher dispatcher;
        private CancellationTokenSource? cts;
        private SemaphoreSlim callbackSemaphore = new SemaphoreSlim(1, 1);
        private Func<bool>? confirmTransferCallback = null;

        public event Action<string>? Error;
        public event Action? BeginTransfer;
        public event Action? Completed;

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
                using(TcpClient client = await server.AcceptTcpClientAsync(ct))
                {
                    string? downloadPath = settingsService.CurrentSettings?.DownloadPath ?? null;
                    if(downloadPath == null || !ValidatePath(ref downloadPath))
                    {
                        HandleError($"Can't write to download path ({downloadPath})!");
                        return;
                    }

                    TransferProtocol protocol = new TransferProtocol(client);

                    TransferMessage msg;
                    msg = await protocol.ReadAsync(ct);
                    if(msg.type != TransferMessage.Type.RequestTransfer)
                    {
                        HandleError($"Unexpected initial message! Expected transfer request!");
                        return;
                    }

                    TransferMessage.Type reqAnswer = TransferMessage.Type.AcceptReceive;
                    await callbackSemaphore.WaitAsync(ct);
                    try
                    {
                        if(confirmTransferCallback != null)
                        {
                            await dispatcher.InvokeAsync(() =>
                            {
                                reqAnswer = confirmTransferCallback.Invoke() ? TransferMessage.Type.AcceptReceive : TransferMessage.Type.DeclineReceive;
                            }, DispatcherPriority.Send, ct);
                        }
                    }
                    finally
                    {
                        callbackSemaphore.Release();
                    }
                    msg = new TransferMessage(reqAnswer);
                    await protocol.SendAsync(msg);

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
                            await protocol.ReadData(path, msg);
                        }
                        else
                        {
                            if(msg.type == TransferMessage.Type.Cancel || msg.type == TransferMessage.Type.Error)
                            {
                                HandleError(msg.type == TransferMessage.Type.Cancel ? "Transfer cancelled by sender!" : "Error occurred at sender!");
                                return;
                            }
                            else if(msg.type == TransferMessage.Type.Complete)
                            {
                                ReportCompleted();
                                break;
                            }
                        }
                    }
                    while(msg.type != TransferMessage.Type.None);
                }
            }
            catch(Exception e)
            {
                if(e is not OperationCanceledException)
                {
                    HandleError($"Could not accept connection ({e.Message})!");
                }
                return;
            }
        }

        private static bool ValidatePath(ref string path)
        {
            try
            {
                path = Path.GetFullPath(path);
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using FileStream fs = File.Create(Path.Combine(path, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                fs.Dispose();
                return true;
            }
            catch { }
            return false;
        }

        private void HandleError(string error)
        {
            dispatcher.Invoke(() =>
            {
                Stop();
                Error?.Invoke(error);
            });
        }

        private void ReportCompleted()
        {
            dispatcher.Invoke(() =>
            {
                Stop();
                Completed?.Invoke();
            });
        }

        public void SetConfirmTransferCallback(Func<bool>? callback)
        {
            callbackSemaphore.Wait();
            confirmTransferCallback = callback;
            callbackSemaphore.Release();
        }
    }
}
