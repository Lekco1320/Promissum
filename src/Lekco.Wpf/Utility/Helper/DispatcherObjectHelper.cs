using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Lekco.Wpf.Utility.Helper
{
    public static class DispatcherObjectHelper
    {
        public static void SafelyDo(this DispatcherObject obj, Action action)
        {
            if (obj.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                obj.Dispatcher.Invoke(action);
            }
        }

        public static Task SafelyBegin(this DispatcherObject obj, Action action, params object[] args)
        {
            if (obj.Dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }
            else
            {
                return obj.Dispatcher.BeginInvoke(action, args).Task;
            }
        }

        public static async Task SafelyDoAsync(this DispatcherObject obj, Action action)
        {
            if (obj.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                await obj.Dispatcher.InvokeAsync(action);
            }
        }

        public static TResult SafelyDo<TResult>(this DispatcherObject obj, Func<TResult> func)
        {
            if (obj.Dispatcher.CheckAccess())
            {
                return func();
            }
            else
            {
                return obj.Dispatcher.Invoke(func);
            }
        }

        public static Task<TResult> SafelyBegin<TResult>(this DispatcherObject obj, Func<TResult> func, params object[] args)
        {
            if (obj.Dispatcher.CheckAccess())
            {
                return Task.FromResult(func());
            }
            else
            {
                return (Task<TResult>)obj.Dispatcher.BeginInvoke(func, args).Task;
            }
        }

        public static async Task<TResult> SafelyDoAsync<TResult>(this DispatcherObject obj, Func<TResult> func)
        {
            if (obj.Dispatcher.CheckAccess())
            {
                return func();
            }
            else
            {
                return await obj.Dispatcher.InvokeAsync(func);
            }
        }
    }
}
