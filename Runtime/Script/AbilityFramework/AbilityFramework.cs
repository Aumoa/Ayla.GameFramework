#nullable enable

using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Ayla
{
    public class AbilityFramework : ScriptableObject
    {
        public const string kDefaultAssetPath = "Assets/Settings/AbilityFramework.asset";

#if !UNITY_EDITOR
        private static AbilityFramework? s_PreloadedAsset;

        private static AbilityFramework PreloadedAsset => s_PreloadedAsset != null ? s_PreloadedAsset : throw new InvalidOperationException("AbilityFramework asset is not preloaded.");
#endif

        public static AbilityFramework Instance
        {
            get
            {
#if UNITY_EDITOR
                return AssetDatabase.LoadAssetAtPath<AbilityFramework>(kDefaultAssetPath);
#else
                return PreloadedAsset;
#endif
            }
        }

        [SerializeField]
        private AttributeSetDefine m_AttributeSet = new();

        public AttributeSetDefine AttributeSet => m_AttributeSet;
    }
}
