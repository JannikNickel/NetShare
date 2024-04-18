using NetShare.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NetShare.Services
{
    public class UdpSearchSenderService(ISettingsService settingsService) : UpdSearchServiceBase(settingsService), ISearchSenderService
    {
        private bool isRunning;
        private UdpClient? client;
        private Timer? sendTimer;

        public void Start()
        {
            if(!isRunning.SetIfChanged(true))
            {
                return;
            }
            client = new UdpClient(port);
            client.EnableBroadcast = true;
            sendTimer = new Timer(async _ => await BroadcastMessage(), null, TimeSpan.Zero, TimeSpan.FromSeconds(interval));
        }

        public void Stop()
        {
            if(!isRunning.SetIfChanged(false))
            {
                return;
            }
            client?.Dispose();
            sendTimer?.Dispose();
        }

        private async Task BroadcastMessage()
        {
            if(!isRunning || client == null)
            {
                return;
            }
            TransferTarget? target = TransferTarget.ForCurrentUser();
            if(target != null)
            {
                string json = JsonSerializer.Serialize(target, serializerOptions);
                byte[] data = encoding.GetBytes(json);
                await client.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));
            }
        }
    }
}
