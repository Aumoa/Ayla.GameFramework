#nullable enable

using System;
using UnityEngine;

namespace Ayla
{
    [Serializable]
    public class AttributeDefine
    {
        [SerializeField]
        private int m_Id;

        public int Id => m_Id;

        [SerializeField]
        private string m_DisplayName = string.Empty;

        public string DisplayName => m_DisplayName;

        [SerializeField]
        private double m_MinValue;

        public double MinValue => m_MinValue;

        [SerializeField]
        private double m_MaxValue;

        public double MaxValue => m_MaxValue;

        [SerializeField]
        private double m_InitialValue;

        public double InitialValue => m_InitialValue;
    }
}
