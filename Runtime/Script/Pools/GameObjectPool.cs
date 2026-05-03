#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    public abstract class GameObjectPool : ScriptableObject
    {
        [SerializeField]
        internal Transform? m_Root;

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            if (m_Root)
            {
                Destroy(m_Root.gameObject);
                m_Root = null;
            }

            if (PoolContainer.TryGetInstance(out var instance))
            {
                instance.InternalUnregisterContainer(this);
            }
        }

        internal abstract void InternalRelease(PooledInstance target);
    }

    public abstract class GameObjectPool<T> : GameObjectPool where T : Object
    {
        [SerializeField]
        private List<PooledInstance> m_Queued = new();

        public T Acquire()
        {
            Asserts.True(ApplicationMisc.IsInMainThread());
            if (m_Queued.Count > 0)
            {
                int index = m_Queued.Count - 1;
                var item = m_Queued[index];
                m_Queued.RemoveAt(index);
                return AcquireObject(item, true);
            }
            else
            {
                PooledInstance.ObjectInitializer.Initialize(this);
                try
                {
                    var pooledInstance = GeneratePooledInstance(true, m_Root);
                    if (pooledInstance.didAwake == false)
                    {
                        throw new InvalidOperationException("PooledInstance must call base.Awake() in its Awake() method.");
                    }
                    return AcquireObject(pooledInstance, false);
                }
                finally
                {
                    PooledInstance.ObjectInitializer.Reset();
                }
            }

            static T AcquireObject(PooledInstance target, bool queued)
            {
                target.OnAcquire(queued);
                if (typeof(T) == typeof(GameObject))
                {
                    return (T)(object)target.gameObject;
                }
                else
                {
                    return target.GetComponent<T>();
                }
            }
        }

        protected abstract PooledInstance GeneratePooledInstance(bool activeSelf, Transform? parent);

        internal override void InternalRelease(PooledInstance target)
        {
            target.OnRelease(false);
            target.transform.SetParent(m_Root, false);
            m_Queued.Add(target);
        }
    }
}
