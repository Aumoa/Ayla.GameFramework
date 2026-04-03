using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ayla
{
    [CreateAssetMenu(fileName = "SingletonDefault.asset", menuName = "Ayla/Singletons/Singleton Default")]
    public class SingletonDefault : ScriptableObject
    {
        public const string kDefaultAssetPath = "Assets/Game/Settings/SingletonDefault.asset";

        [SerializeField, HideInInspector]
        private OrderedDictionary<string, SingletonData> m_SingletonDatas;

        private readonly Dictionary<Type, SingletonData> m_DataMapCache = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_DataMapCache.Clear();
        }
#endif

        public SingletonData? GetData(Singleton behaviour)
        {
            return GetData(behaviour.GetType());
        }

        public SingletonData? GetData(Type behaviourType)
        {
            if (m_DataMapCache.TryGetValue(behaviourType, out var data))
            {
                return data;
            }

            var dataType = Singleton.GetDataType(behaviourType);
            if (dataType == null)
            {
                throw new ArgumentException("Singleton is not contains TData.", nameof(behaviourType));
            }

            foreach (var (_, data2) in m_SingletonDatas)
            {
                if (data2.GetType() == dataType)
                {
                    m_DataMapCache[behaviourType] = data2;
                    return data2;
                }
            }

            return null;
        }
    }
}
