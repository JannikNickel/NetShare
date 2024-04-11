using NetShare.Models;
using System;
using System.Collections.Generic;

namespace NetShare.Services
{
    public interface IBroadcastSearchService
    {
        void Start();
        void Stop();
    }
}
