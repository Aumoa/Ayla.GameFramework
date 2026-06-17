#nullable enable

using System;
using UnityEngine;

namespace Ayla
{
    [Serializable]
    public class AttributeSet
    {
        [SerializeField]
        private AbilityComponent? m_Owner;

        [SerializeField]
        private RuntimeAttribute[] m_RuntimeAtts = Array.Empty<RuntimeAttribute>();

        internal void Initialize(AbilityComponent owner)
        {
            m_Owner = owner;

            var attrs = AbilityFramework.Instance.AttributeSet.Attributes;
            m_RuntimeAtts = new RuntimeAttribute[attrs.Count];
            for (int i = 0; i < attrs.Count; i++)
            {
                m_RuntimeAtts[i] = new RuntimeAttribute(i, attrs[i]);
            }
        }
    }
}
