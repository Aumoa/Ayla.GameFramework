#nullable enable

using UnityEngine;

namespace Ayla
{
    public class AbilitySystemData : ScriptableObject
    {
        [SerializeField]
        private AttributeSetDefine m_AttributeSet = new();
    }
}
