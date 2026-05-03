#nullable enable

using System;
using UnityEngine;

namespace Ayla
{
    [DisallowMultipleComponent]
    public class PooledInstance : MonoBehaviour
    {
        internal static class ObjectInitializer
        {
            public static GameObjectPool? s_Container;

            public static void Initialize(GameObjectPool container)
            {
                s_Container = container;
            }

            public static void Reset()
            {
                s_Container = null;
            }
        }

        [SerializeField]
        private GameObjectPool m_Container = ObjectInitializer.s_Container!;

        internal protected virtual void OnAcquire(bool queued)
        {
            if (queued)
            {
                gameObject.SetActive(true);
            }
        }

        internal protected virtual void OnRelease(bool finalize)
        {
            if (finalize == false)
            {
                gameObject.SetActive(false);
            }
        }

        public static void Release(GameObject target)
        {
            if (target == null || target.TryGetComponent<PooledInstance>(out var pooledInstance) == false)
            {
                throw new ArgumentException("The target GameObject does not have a PooledInstance component.");
            }

            pooledInstance.m_Container.InternalRelease(pooledInstance);
        }

        public static void Release(Component target)
        {
            if (target == null || target.TryGetComponent<PooledInstance>(out var pooledInstance) == false)
            {
                throw new ArgumentException("The target Component does not have a PooledInstance component.");
            }

            pooledInstance.m_Container.InternalRelease(pooledInstance);
        }

        public static void Release(PooledInstance target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "The target PooledInstance cannot be null.");
            }

            target.m_Container.InternalRelease(target);
        }
    }
}
