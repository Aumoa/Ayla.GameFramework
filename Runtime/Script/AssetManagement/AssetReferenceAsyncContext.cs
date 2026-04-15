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

        public abstract double Progress { get; }

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
        private Func<double>? m_ProgressGetter;

        private readonly AssetReferenceAsyncContext? m_FromHandle;

        public override double Progress
        {
            get
            {
                if (Task.IsCompleted)
                {
                    return 1;
                }

                if (m_FromHandle != null)
                {
                    return m_FromHandle.Progress;
                }

                return m_ProgressGetter?.Invoke() ?? 0;
            }
        }

        internal AssetReferenceAsyncContext(T result)
        {
            Task = Task2.FromResult(result);
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
                    m_ProgressGetter = () => asyncOp.progress;
                    var result = await task.WaitAsync(cancellationToken);
                    return result[0];
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
