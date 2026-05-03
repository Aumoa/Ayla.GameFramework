using UnityEngine;

namespace Ayla
{
    public class GameObjectPrefabPool : PrefabPool<GameObjectPrefabPool, GameObject>
    {
        public static GameObjectPrefabPool Create(GameObject targetPrefab)
        {
            return InvokeCreate(targetPrefab);
        }
    }
}
