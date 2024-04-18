using NetShare.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NetShare.Services
{
    public class UdpSearchListenerService(ISettingsService settingsService) : UpdSearchServiceBase(settingsService), ISearchListenerService
    {
        private bool isRunning;
        private Dispatcher? dispatcher;
        private UdpClient? client;
        private Timer? decayTimer;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly List<TransferTarget> targets = new List<TransferTarget>();
        private readonly Dictionary<IPAddress, DateTime> answerTimes = new Dictionary<IPAddress, DateTime>();

        public event Action<IReadOnlyCollection<TransferTarget>>? TargetsChanged;

        public void Start()
        {
            if(!isRunning.SetIfChanged(true))
            {
                return;
            }
            dispatcher = Dispatcher.CurrentDispatcher;
            client = new UdpClient(port);
            decayTimer = new Timer(_ => DecayEntries(), null, TimeSpan.Zero, TimeSpan.FromSeconds(interval));
            BeginListen();
        }

        public void Stop()
        {
            if(!isRunning.SetIfChanged(false))
            {
                return;
            }
            client?.Dispose();
            decayTimer?.Dispose();
        }

        private async void BeginListen()
        {
            while(isRunning && client != null)
            {
                try
                {
                    UdpReceiveResult res = await client.ReceiveAsync();
                    string json = encoding.GetString(res.Buffer);
                    TransferTarget? target = JsonSerializer.Deserialize<TransferTarget>(json, serializerOptions);
                    if(target != null)
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            int existing = targets.FindIndex(n => n.Ip.Equals(target.Ip));
                            if(existing != -1)
                            {
                                targets[existing] = target;
                            }
                            else
                            {
                                targets.Add(target);
                            }
                            answerTimes[target.Ip] = DateTime.UtcNow;
                            await DispatchTargets();
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                }
                catch { }
            }
        }

        private async void DecayEntries()
        {
            if(!isRunning)
            {
                return;
            }

            semaphore.Wait();
            try
            {
                int oldCount = targets.Count;
                DateTime now = DateTime.UtcNow;
                for(int i = targets.Count - 1;i >= 0;i--)
                {
                    if((now - answerTimes[targets[i].Ip]).TotalSeconds >= decayTime)
                    {
                        targets.RemoveAt(i);
                    }
                }
                if(targets.Count != oldCount)
                {
                    await DispatchTargets();
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task DispatchTargets()
        {
            if(dispatcher != null)
            {
                await dispatcher.InvokeAsync(() => TargetsChanged?.Invoke(targets.AsReadOnly()));
            }
        }
    }
}
