#nullable enable

using System;
using UnityEngine;

namespace Ayla
{
    [Serializable]
    public class RuntimeAttribute
    {
        [SerializeField]
        private int m_DefineIndex;
        [SerializeField]
        private double m_Value;

        public RuntimeAttribute()
        {
        }

        internal RuntimeAttribute(int defineIndex, AttributeDefine define)
        {
            m_DefineIndex = defineIndex;
            m_Value = define.InitialValue;
        }

        public double Value
        {
            get => m_Value;
            set
            {
                var define = AbilityFramework.Instance.AttributeSet.Attributes[m_DefineIndex];
                m_Value = Math.Clamp(value, define.MinValue, define.MaxValue);
            }
        }
    }
}
