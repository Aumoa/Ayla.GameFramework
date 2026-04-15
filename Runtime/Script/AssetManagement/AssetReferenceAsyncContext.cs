#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Task2 = System.Threading.Tasks.Task;

namespace Ayla
{
    public abstract class AssetReferenceAsyncContext
    {
        protected bool ReleaseHandleOnCompletionQueued { get; private set; }

        public virtual double Progress { get; protected set; }

        public bool IsDone => Progress >= 1.0;

        public abstract Task2 WaitAsync(CancellationToken cancellationToken = default);

        public abstract void Release();

        public void ReleaseHandleOnCompletion()
        {
            if (IsDone)
            {
                Release();
                return;
            }

            if (!ReleaseHandleOnCompletionQueued)
            {
                WaitAsync().ContinueWith(_ => Release());
                ReleaseHandleOnCompletionQueued = true;
            }
        }
    }

    public partial class AssetReferenceAsyncContext<T> : AssetReferenceAsyncContext where T : Object
    {
        private event Action? ReleaseAction;

        private AssetReferenceAsyncContext? m_FromHandle;

        public override double Progress
        {
            get => m_FromHandle?.Progress ?? base.Progress;
            protected set => base.Progress = value;
        }

        internal AssetReferenceAsyncContext(T result)
        {
            Task = Task2.FromResult(result);
            Progress = 1;
        }

        private AssetReferenceAsyncContext(AssetReferenceAsyncContext from, Task2 task)
        {
            m_FromHandle = from;
            Task = (Task<T>)task;
        }

        internal AssetReferenceAsyncContext(AsyncInstantiateOperation<T> asyncOp, CancellationToken cancellationToken)
        {
            Task = Start();

            return;

            async Task<T> Start()
            {
                var task = TaskUtility.Create(async () => await asyncOp).AsTask();
                try
                {
                    while (!asyncOp.isDone)
                    {
                        Progress = (double)asyncOp.progress;
                        await Task2.WhenAny(task, Task2.Delay(TimeSpan.FromSeconds(0.1), cancellationToken));
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    Progress = 1;
                    return asyncOp.Result[0];
                }
                catch
                {
                    _ = task.ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            var result = t.Result[0];
                            if (result)
                            {
                                Object.Destroy(result);
                            }
                        }
                    });

                    throw;
                }
            }
        }

        public Task<T> Task { get; }

        public T Result => Task.Result;

        public override Task2 WaitAsync(CancellationToken cancellationToken = default)
        {
            return Task.WaitAsync(cancellationToken);
        }

        public override void Release()
        {
            var invocable = ReleaseAction;
            ReleaseAction = null;
            invocable?.Invoke();
        }

        public AssetReferenceAsyncContext<U> As<U>() where U : Object
        {
            return new AssetReferenceAsyncContext<U>(this, Task.ContinueWith(t => (U)(object)t.Result));
        }
    }
}
