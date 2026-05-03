#nullable enable

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ayla
{
    internal static class PrefabPool
    {
        internal static Object? s_TargetPrefabAsset;

        public static TPoolClass CreatePool<TPoolClass, TPrefab>(TPrefab prefab)
            where TPoolClass : GameObjectPool
            where TPrefab : Object
        {
            if (typeof(TPrefab) != typeof(GameObject) && typeof(TPrefab).IsAssignableTo(typeof(Component)) == false)
            {
                throw new ArgumentException("Prefab must be a GameObject or a Component.");
            }

            if (typeof(TPoolClass).IsImplements(typeof(PrefabPool<,>)) == false)
            {
                throw new ArgumentException($"Pool class must inherit from {nameof(PrefabPool)}<T>.");
            }

            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            s_TargetPrefabAsset = prefab;
            try
            {
                return PoolContainer.Instance.CreatePool<TPoolClass>($"PrefabPool: {prefab.name}");
            }
            finally
            {
                s_TargetPrefabAsset = null;
            }
        }
    }

    public abstract class PrefabPool<TPoolClass, TPrefab> : GameObjectPool<TPrefab>
        where TPoolClass : GameObjectPool
        where TPrefab : Object
    {
        private TPrefab? m_TargetPrefab = (TPrefab?)(object?)PrefabPool.s_TargetPrefabAsset;

        protected override void OnEnable()
        {
            base.OnEnable();
            Asserts.True(m_TargetPrefab || PrefabPool.s_TargetPrefabAsset);
        }

        protected override PooledInstance GeneratePooledInstance(bool activeSelf, Transform? parent)
        {
            Asserts.True(typeof(TPrefab) == typeof(GameObject) || typeof(TPrefab).IsAssignableTo(typeof(Component)));
            Asserts.True(m_TargetPrefab != null);

            var targetObject = Instantiate(m_TargetPrefab, parent);
            GameObject gameObject;
            if (targetObject is Component component)
            {
                gameObject = component.gameObject;
            }
            else
            {
                gameObject = (GameObject)(object)targetObject;
            }

            var pooledInstance = gameObject.AddComponent<PooledInstance>();
            if (activeSelf == false)
            {
                gameObject.SetActive(false);
            }

            return pooledInstance;
        }

        protected static TPoolClass InvokeCreate(TPrefab targetPrefab)
        {
            return PrefabPool.CreatePool<TPoolClass, TPrefab>(targetPrefab);
        }
    }
}
