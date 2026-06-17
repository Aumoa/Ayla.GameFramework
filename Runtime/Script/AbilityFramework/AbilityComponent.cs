#nullable enable

using UnityEngine;

namespace Ayla
{
    public class AbilityComponent : MonoBehaviour
    {
        [SerializeField]
        private AttributeSet m_AttributeSet = new();

        private void Awake()
        {
            m_AttributeSet.Initialize(this);
        }
    }
}
