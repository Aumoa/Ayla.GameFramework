using UnityEngine;

namespace Ayla
{
    [CreateAssetMenu(fileName = "SingletonDefault.asset", menuName = "Ayla/Singletons/Singleton Default")]
    public class SingletonDefault : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private OrderedDictionary<string, SingletonData> m_SingletonDatas;
    }
}
