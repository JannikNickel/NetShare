using System;
using System.Threading;
using System.Windows.Threading;

namespace NetShare
{
    public sealed class DispatcherLimiter : IDisposable
    {
        private readonly Dispatcher dispatcher;
        private readonly Timer timer;
        private readonly object latestLock = new object();
        private Action? latest;

        public DispatcherLimiter(Dispatcher dispatcher, double updateRate)
        {
            this.dispatcher = dispatcher;
            timer = new Timer(DispatchLatest, null, TimeSpan.Zero, TimeSpan.FromSeconds(updateRate));
        }

        public void Invoke(Action callback, bool now = false)
        {
            if(now)
            {
                dispatcher.Invoke(callback);
                return;
            }
            lock(latestLock)
            {
                latest = callback;
            }
        }

        private void DispatchLatest(object? state)
        {
            Action? callback;
            lock(latestLock)
            {
                callback = latest;
                latest = null;
            }
            if(callback != null)
            {
                dispatcher.Invoke(callback);
            }
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
