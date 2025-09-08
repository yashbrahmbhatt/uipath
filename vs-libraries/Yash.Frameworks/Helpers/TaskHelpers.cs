using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Frameworks.Helpers
{
    public static class TaskHelpers
    {
        public static IAsyncResult BeginTask<T>(Task<T> task, AsyncCallback callback, object state, Action<string, TraceEventType>? log = null)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            log?.Invoke($"[TaskHelpers] BeginTask<{typeof(T).Name}> started.", TraceEventType.Verbose);

            var tcs = new TaskCompletionSource<T>(state);

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var ex = t.Exception?.InnerException ?? t.Exception!;
                    log?.Invoke($"[TaskHelpers] Task faulted: {ex.Message}", TraceEventType.Error);
                    tcs.TrySetException(ex);
                }
                else if (t.IsCanceled)
                {
                    log?.Invoke("[TaskHelpers] Task was canceled.", TraceEventType.Warning);
                    tcs.TrySetCanceled();
                }
                else
                {
                    log?.Invoke($"[TaskHelpers] Task completed successfully with result: {t.Result}", TraceEventType.Verbose);
                    tcs.TrySetResult(t.Result);
                }

                callback?.Invoke(tcs.Task);
            }, TaskScheduler.Default);

            return tcs.Task;
        }

        public static T EndTask<T>(IAsyncResult asyncResult, Action<string, TraceEventType>? log = null)
        {
            if (asyncResult is not Task<T> task)
                throw new ArgumentException("Invalid IAsyncResult type.", nameof(asyncResult));

            log?.Invoke($"[TaskHelpers] Ending task and awaiting result of type: {typeof(T).Name}", TraceEventType.Verbose);
            return task.Result; // Rethrows any exceptions
        }

        public static void EndTask(IAsyncResult asyncResult, Action<string, TraceEventType>? log = null)
        {
            if (asyncResult is not Task task)
                throw new ArgumentException("Invalid IAsyncResult type.", nameof(asyncResult));
            log?.Invoke("[TaskHelpers] Ending task with no return value.", TraceEventType.Verbose);
            if(task.Exception != null) throw task.Exception;
        }
    }
}
