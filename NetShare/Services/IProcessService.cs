using NetShare.Models;
using System;
using System.Collections.Generic;

namespace NetShare.Services
{
    public interface IProcessService
    {
        void Start();
        void Stop();
    }
}
