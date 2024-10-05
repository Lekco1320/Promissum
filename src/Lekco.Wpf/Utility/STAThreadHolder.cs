using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Lekco.Wpf.Utility.Helper;

namespace Lekco.Wpf.Utility
{
    public class STAThreadHolder<T> : IDisposable where T : DispatcherObject
    {
        protected Thread STAThread { get; }

        protected T DispatcherObject { get; set; }

        private bool _disposed;

        private readonly ManualResetEvent _initialized = new ManualResetEvent(false);

#nullable disable
        public STAThreadHolder(Func<T> initFunc)
#nullable enable
        {
            STAThread = new Thread(() =>
            {
                DispatcherObject = initFunc();
                _initialized.Set();
                Dispatcher.Run();
            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            _initialized.WaitOne();
        }

        private void EnsureInitialized()
        {
            _initialized.WaitOne();
        }

        public void SafelyDo(Action<T> action)
        {
            EnsureInitialized();
            DispatcherObject.SafelyDo(() => action(DispatcherObject));
        }

        public Task SafelyBegin(Action<T> action, params object[] args)
        {
            EnsureInitialized();
            return DispatcherObject.SafelyBegin(() => action(DispatcherObject), args);
        }

        public TResult SafelyDo<TResult>(Func<T, TResult> func)
        {
            EnsureInitialized();
            return DispatcherObject.SafelyDo(() => func(DispatcherObject));
        }

        public Task<TResult> SafelyBegin<TResult>(Func<T, TResult> func, params object[] args)
        {
            EnsureInitialized();
            return DispatcherObject.SafelyBegin(() => func(DispatcherObject), args);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (DispatcherObject?.Dispatcher != null)
                {
                    DispatcherObject.Dispatcher.InvokeShutdown();
                }
                if (STAThread != null && STAThread.IsAlive)
                {
                    STAThread.Join();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~STAThreadHolder()
        {
            Dispose(false);
        }
    }
}
