/*
 * This template is originally from
 * https://github.com/DanielWillett/SampleRocketPlugin
 */

using Rocket.Core.Utils;
using System.Runtime.CompilerServices;
using Action = System.Action;

namespace SampleRocketPlugin.Util;

public static class ThreadEx
{
    /// <summary><see langword="await"/> this function to be taken to the main thread.</summary>
    public static MainThreadTask ToMainThread(CancellationToken token = default) => new MainThreadTask(token);

    public static void RunTask<T1, T2, T3>(Func<T1, T2, T3, CancellationToken, Task> task, T1 arg1, T2 arg2, T3 arg3, CancellationToken token = default, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task(arg1, arg2, arg3, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        }, token);
    }
    public static void RunTask<T1, T2, T3>(Func<T1, T2, T3, Task> task, T1 arg1, T2 arg2, T3 arg3, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task(arg1, arg2, arg3).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        });
    }
    public static void RunTask<T1, T2>(Func<T1, T2, CancellationToken, Task> task, T1 arg1, T2 arg2, CancellationToken token = default, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task(arg1, arg2, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        }, token);
    }
    public static void RunTask<T1, T2>(Func<T1, T2, Task> task, T1 arg1, T2 arg2, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task(arg1, arg2).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        });
    }
    public static void RunTask<T>(Func<T, CancellationToken, Task> task, T arg1, CancellationToken token = default, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task(arg1, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        }, token);
    }
    public static void RunTask<T>(Func<T, Task> task, T arg1, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task(arg1).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        });
    }
    public static void RunTask(Func<CancellationToken, Task> task, CancellationToken token = default, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task(token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        }, token);
    }
    public static void RunTask(Func<Task> task, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        });
    }
    public static void RunTask(Task task, string? ctx = null, [CallerMemberName] string member = "", [CallerFilePath] string fp = "", bool awaitOnUnload = false, int timeout = 180000)
    {
        Task.Run(async () =>
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(ctx))
                    ctx = member;
                else
                    ctx = "\"" + ctx + "\" Member: " + member;
                Logger.LogException(ex, "Exception in task: " + ctx + ".");
            }
        });
    }
}

public class MainThreadTask
{
    internal const int DefaultTimeout = 5000;
    protected volatile bool IsCompleted;
    protected readonly MainThreadResult Awaiter;
    public readonly CancellationToken Token;
    
    public MainThreadTask(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        this.Token = token;
        Awaiter = new MainThreadResult(this);
    }
    public MainThreadResult GetAwaiter()
    {
        return Awaiter;
    }
    public sealed class MainThreadResult : INotifyCompletion
    {
        internal Action? Continuation;
        public readonly MainThreadTask Task;
        public MainThreadResult(MainThreadTask task)
        {
            this.Task = task ?? throw new ArgumentNullException(nameof(task), "Task was null in MainThreadResult constructor.");
        }
        public bool IsCompleted => ThreadUtil.gameThread == Thread.CurrentThread || Task.IsCompleted;
        public void OnCompleted(Action continuation)
        {
            Task.Token.ThrowIfCancellationRequested();
            if (ThreadUtil.gameThread == Thread.CurrentThread)
            {
                continuation();
                Task.IsCompleted = true;
            }
            else
            {
                this.Continuation = continuation;
                TaskDispatcher.QueueOnMainThread(continuation);
            }
        }
        internal void Complete()
        {
            Task.IsCompleted = true;
        }

        private bool WaitCheck() => Task.IsCompleted || Task.Token.IsCancellationRequested;
        public void GetResult()
        {
            if (ThreadUtil.gameThread == Thread.CurrentThread)
                return;
            SpinWait.SpinUntil(WaitCheck, DefaultTimeout);
        }
    }
}