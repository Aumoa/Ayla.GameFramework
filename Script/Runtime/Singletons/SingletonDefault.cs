using System;
using UnityEngine;

namespace Ayla
{
    [CreateAssetMenu(fileName = "SingletonDefault.asset", menuName = "Ayla/Singletons/Singleton Default")]
    public class SingletonDefault : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private OrderedDictionary<string, SingletonData> m_SingletonDatas;

        public SingletonData GetData(Singleton behaviour)
        {
            var dataType = Singleton.GetDataType(behaviour.GetType());
            if (dataType == null)
            {
                throw new ArgumentException("Singleton is not contains TData.", nameof(behaviour));
            }

            foreach (var (_, data) in m_SingletonDatas)
            {
                if (data.GetType() == dataType)
                {
                    return data;
                }
            }

            throw new ArgumentException("No data found for the specified Singleton type.", nameof(behaviour));
        }
    }
}
