using NetShare.Models;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using NetShare.Converters;
using System.Windows.Threading;
using System.Collections.Generic;

namespace NetShare.Services
{
    public abstract class UpdSearchServiceBase
    {
        protected const double interval = 1.0;
        protected const double decayTime = 3.0;

        protected readonly int port;
        protected readonly JsonSerializerOptions serializerOptions;
        protected readonly Encoding encoding;

        public UpdSearchServiceBase(ISettingsService settingsService)
        {
            this.port = settingsService.CurrentSettings?.BroadcastPort ?? 0;
            serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new IpAddressJsonConverter());
            serializerOptions.WriteIndented = false;
            encoding = Encoding.UTF8;
        }
    }
}
