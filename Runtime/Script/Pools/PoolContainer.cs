#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Ayla
{
    public class PoolContainer : Singleton<PoolContainer>
    {
        [SerializeField]
        private List<GameObjectPool> m_Pools = new();

        public T CreatePool<T>(string? displayName = null) where T : GameObjectPool
        {
            var pool = CreateInstance<T>();
            try
            {
                pool.m_Root = new GameObject(displayName ?? typeof(T).Name).transform;
                pool.m_Root.SetParent(Manager.transform, false);
                m_Pools.Add(pool);
                return pool;
            }
            catch
            {
                Destroy(pool);
                throw;
            }
        }

        internal void InternalUnregisterContainer(GameObjectPool pool)
        {
            m_Pools.Remove(pool);
        }
    }
}
