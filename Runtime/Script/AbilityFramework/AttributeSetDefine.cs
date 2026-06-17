#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ayla
{
    [Serializable]
    public class AttributeSetDefine
    {
        [SerializeField]
        private AttributeDefine[] m_Attributes = Array.Empty<AttributeDefine>();

        public IReadOnlyList<AttributeDefine> Attributes => m_Attributes;
    }
}
